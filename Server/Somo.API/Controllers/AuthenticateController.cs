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
                id = user.Id.ToString()
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
            return Ok(new {
                id = user.Id,
                username = user.UserName,
                email = user.Email
            });
        }
    }
}