using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace HeroesAPI.Controllers
{
    [LicenseProvider(typeof(LicFileLicenseProvider))]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly SignInManager<IdentityUser> _signInManager;


        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger, IAuthRepository authRepository, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _authRepository = authRepository;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] UserRegister userRegister)
        {
            try
            {
                if (userRegister is null)
                {
                    return NotFound();
                }

                var response = await _authRepository.RegisterAsync(userRegister);

                if (response.Status == "999")
                {
                    return BadRequest(response.Message);
                }

                return Ok("Your registration is successful " + userRegister.Email);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost("confirmAccount")]
        public async Task<ActionResult> ConfirmEmailAsync(string userId, string code)
        {
            try
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
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost("registerAdmin")]
        public async Task<ActionResult> RegisterAdmin([FromBody] UserRegister userRegister)
        {
            try
            {
                if (userRegister is null)
                {
                    return NotFound();
                }

                var response = await _authRepository.RegisterAdminAsync(userRegister);

                if (response.Status == "999")
                {
                    return BadRequest(response.Message);
                }

                return Ok("Your registration as admin is successful " + userRegister.Email);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLogin userLogin)
        {
            try
            {
                Response? response = await _authRepository.LoginAsync(userLogin);

                if (response == null)
                {
                    return BadRequest("Response is null");
                }
                else
                {
                    return Ok(response.Message);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }




        [HttpPost("change-password"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] string oldPassword, string newPassword)
        {
            try
            {
                string? userName = User.FindFirstValue(ClaimTypes.Name);

                IdentityUser? user = await _userManager.FindByNameAsync(userName);

                if (user is null)
                {
                    return NotFound("User not found");
                }

                await _authRepository.ChangePasswordAsync(user, oldPassword, newPassword);
                return Ok("Password has been changed successfully");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromForm] string email)
        {
            try
            {
                Response? response = await _authRepository.ForgotPasswordAsync(email);

                if (response == null)
                {
                    return BadRequest("Response is null");
                }
                else
                {
                    return Ok($"Successful");
                }

            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromForm] string email, string code, string newPassword)
        {
            try
            {
                IdentityUser? userExists = await _userManager.FindByEmailAsync(email);

                if (userExists == null)
                {
                    return BadRequest("User do not exist");
                }

                byte[]? decodedToken = WebEncoders.Base64UrlDecode(code);
                string normalToken = Encoding.UTF8.GetString(decodedToken);

                Response? response = await _authRepository.ResetPasswordAsync(userExists, normalToken, newPassword);

                if (response == null)
                {
                    return BadRequest("Response is null");
                }
                else
                {
                    return Ok($"Response is successful");
                }

            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost("logout-all-users")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                Response? response = await _authRepository.LogoutAsync();

                if (response == null)
                {
                    return BadRequest("Response is null");
                }
                else
                {
                    return Ok($"Response is successful");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

    }
}