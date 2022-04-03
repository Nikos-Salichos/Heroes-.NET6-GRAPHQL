using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(IAuthRepository authRepository, UserManager<IdentityUser> userManager)
        {
            _authRepository = authRepository;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] UserRegister userRegister)
        {
            if (userRegister is null)
            {
                return NotFound();
            }

            var response = await _authRepository.Register(userRegister);

            if (response.Status == "999")
            {
                return BadRequest(response.Message);
            }

            return Ok("Your registration is successful " + userRegister.Email);
        }

        [HttpPost("confirmAccount")]
        public async Task<ActionResult> ConfirmEmailAsync(string userId, string code)
        {
            IdentityUser? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }

            byte[]? decodedToken = WebEncoders.Base64UrlDecode(code);
            string token = Encoding.UTF8.GetString(decodedToken);
            IdentityResult? result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("registerAdmin")]
        public async Task<ActionResult> RegisterAdmin([FromBody] UserRegister userRegister)
        {
            if (userRegister is null)
            {
                return NotFound();
            }

            var response = await _authRepository.RegisterAdmin(userRegister);

            if (response.Status == "999")
            {
                return BadRequest(response.Message);
            }

            return Ok("Your registration as admin is successful " + userRegister.Email);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            string? response = await _authRepository.Login(userLogin);


            if (string.IsNullOrWhiteSpace(response))
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("change-password"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] string oldPassword, string newPassword)
        {
            string? userName = User.FindFirstValue(ClaimTypes.Name);

            //  Nikos1234$
            IdentityUser? user = await _userManager.FindByNameAsync(userName);

            if (user is null)
            {
                return NotFound("User not found");
            }

            await _authRepository.ChangePassword(user, oldPassword, newPassword);
            return Ok("Password has been changed successfully");
        }

    }
}