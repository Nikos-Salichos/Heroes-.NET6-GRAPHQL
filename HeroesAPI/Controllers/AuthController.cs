using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromForm] UserRegister userRegister)
        {
            if (userRegister is null)
            {
                return NotFound();
            }

            var response = await _authRepository.Register(new User
            {
                Email = userRegister.Email
            },
            userRegister.Password);


            if (string.IsNullOrWhiteSpace(response))
            {
                return BadRequest("User already exists");
            }

            return Ok("Your registration is successful " + userRegister.Email);
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLogin userLogin)
        {
            string? response = await _authRepository.Login(userLogin.Email, userLogin.Password);

            if (string.IsNullOrWhiteSpace(response))
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("change-password"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] string newPassword)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return NotFound("User not found");
            }

            if (!int.TryParse(userId, out int userIdentifier))
            {
                return NotFound("User not found");
            }

            await _authRepository.ChangePassword(userIdentifier, newPassword);
            return Ok("Password has been changed successfully");
        }

    }
}