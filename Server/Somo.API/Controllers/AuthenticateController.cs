using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Somo.Domain.Entities;
using Somo.API.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Server.Models;
using Microsoft.AspNetCore.Authorization;
using Somo.Application.Common;
using Somo.Application.DTOs;
using Somo.Application.Interfaces;

namespace Somo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager, 
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("id", user.Id.ToString()), 
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                username = user.UserName,
                email = user.Email,
                roles = userRoles,
                id = user.Id.ToString(),
                firstName = user.FirstName ?? string.Empty,
                lastName = user.LastName ?? string.Empty,
                phone = user.PhoneNumber ?? string.Empty,
                profilePhotoUrl = user.ProfilePhotoUrl
            });
            }
            return Unauthorized("Invalid username or password");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterModel model,
            [FromServices] IClinicRegistrationService clinicRegistration)
        {
            var roleName = model.Role ?? AppRoles.Owner;

            if (!AppRoles.SelfService.Contains(roleName))
                return BadRequest(new { Status = "Error", Message = "Tip de cont invalid." });

            if (roleName == AppRoles.ClinicAdmin && !IsClinicPayloadValid(model.Clinic))
                return BadRequest(new { Status = "Error", Message = "Completează datele cabinetului: nume, adresă completă, contact, orar și cel puțin un medic veterinar." });

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Conflict(new { Status = "Error", Message = "Există deja un cont cu acest username." });

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Status = "Error", Message = "Contul nu a putut fi creat: " + errors });
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            await _userManager.AddToRoleAsync(user, roleName);

            if (roleName == AppRoles.ClinicAdmin)
            {
                try
                {
                    await clinicRegistration.SubmitAsync(model.Clinic!, user.Id.ToString());
                }
                catch
                {
                    await _userManager.DeleteAsync(user);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "Cererea pentru cabinet nu a putut fi înregistrată. Încearcă din nou." });
                }

                return Ok(new
                {
                    Status = "PendingApproval",
                    Message = "Cererea a fost trimisă. Un administrator Somo o va verifica în curând."
                });
            }

            return Ok(new { Status = "Success", Message = "Cont creat cu succes." });
        }

        private static bool IsClinicPayloadValid(RegisterClinicDto? clinic)
        {
            if (clinic is null)
                return false;

            var required = new[]
            {
                clinic.Name, clinic.Street, clinic.StreetNumber,
                clinic.City, clinic.County, clinic.Phone,
                clinic.Email, clinic.Schedule
            };

            if (required.Any(string.IsNullOrWhiteSpace))
                return false;

            return clinic.VetNames.Any(v => !string.IsNullOrWhiteSpace(v));
        }
        
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
        [HttpGet("user/{userId}")]
        [Authorize(Roles = AppRoles.ClinicAdmin + "," + AppRoles.SomoAdmin)]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();
            return Ok(ToProfile(user));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized(new { Status = "Error", Message = "Token invalid." });

            return Ok(ToProfile(user));
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized(new { Status = "Error", Message = "Token invalid." });

            var email = dto.Email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { Status = "Error", Message = "Adresa de email este obligatorie." });

            if (!string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var otherUser = await _userManager.FindByEmailAsync(email);
                if (otherUser != null && otherUser.Id != user.Id)
                    return Conflict(new { Status = "Error", Message = "Există deja un cont cu această adresă de email." });

                user.Email = email;
                user.NormalizedEmail = _userManager.NormalizeEmail(email);
            }

            user.FirstName = dto.FirstName?.Trim();
            user.LastName = dto.LastName?.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
            user.ProfilePhotoUrl = string.IsNullOrWhiteSpace(dto.ProfilePhotoUrl) ? null : dto.ProfilePhotoUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Status = "Error", Message = "Datele nu au putut fi salvate: " + errors });
            }

            return Ok(ToProfile(user));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized(new { Status = "Error", Message = "Token invalid." });

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { Status = "Error", Message = "Completează parola curentă și pe cea nouă." });

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Status = "Error", Message = "Parola nu a putut fi schimbată: " + errors });
            }

            return Ok(new { Status = "Success", Message = "Parola a fost schimbată." });
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = User.FindFirst("id")?.Value;
            return string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
        }

        private static UserProfileDto ToProfile(ApplicationUser user) => new()
        {
            Id = user.Id.ToString(),
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        };
    }
}