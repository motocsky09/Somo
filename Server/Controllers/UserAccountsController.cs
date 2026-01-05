using Microsoft.AspNetCore.Mvc;
using Somo.Server.Entities;
using Somo.Server.Repositories;
using System.Threading.Tasks;

namespace Somo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAccountsController : ControllerBase
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public UserAccountsController(IUserAccountRepository userAccountRepository)
        {
            _userAccountRepository = userAccountRepository;
        }

        // POST: api/useraccounts/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Invalid client request");
            }

            var user = await _userAccountRepository.GetByUsernameAsync(login.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            // For now, just return the user. Later, we can implement JWT token generation.
            return Ok(user);
        }
    }

    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
