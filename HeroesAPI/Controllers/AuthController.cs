using HeroesAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;

namespace HeroesAPI.Controllers
{
    [LicenseProvider(typeof(LicFileLicenseProvider))]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger, IUnitOfWorkRepository unitOfWorkRepository, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _unitOfWorkRepository = unitOfWorkRepository;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] UserRegister userRegister)
        {

            if (userRegister is null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name);
            }

            ApiResponse? response = await _unitOfWorkRepository.AuthRepository.RegisterAsync(userRegister);

            if (response == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response");
            }

            if (!response.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + response.Message.FirstOrDefault());
            }
            else
            {
                return Ok("Your registration is successful " + userRegister.Email);
            }
        }

        [HttpPost("confirmAccount")]
        public async Task<ActionResult> ConfirmEmailAsync(string userId, string code)
        {

            IdentityUser? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name);
            }

            byte[]? decodedToken = WebEncoders.Base64UrlDecode(code);
            string token = Encoding.UTF8.GetString(decodedToken);
            IdentityResult? result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok("Account confirmed");
            }
            else
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response");
            }

        }

        [HttpPost("registerAdmin")]
        public async Task<ActionResult> RegisterAdmin([FromBody] UserRegister userRegister)
        {

            if (userRegister is null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + "user not found");
            }

            ApiResponse? response = await _unitOfWorkRepository.AuthRepository.RegisterAdminAsync(userRegister);

            if (response == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response");
            }

            if (!response.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + response.Message.FirstOrDefault());
            }
            else
            {
                return Ok("Your registration as admin is successful " + userRegister.Email);
            }

        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLogin userLogin)
        {
            ApiResponse? response = await _unitOfWorkRepository.AuthRepository.LoginAsync(userLogin);

            if (response == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response");
            }

            if (!response.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + response.Message.FirstOrDefault());
            }
            else
            {
                return Ok(response.Message);
            }
        }

        [HttpPost("validate-tfa")]
        public async Task<ActionResult> ValidateTFA(string userName, string tfaToken)
        {
            IdentityUser? user = await _userManager.FindByNameAsync(userName);

            if (user is null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name);
            }

            ApiResponse? apiResponse = await _unitOfWorkRepository.AuthRepository.ValidateTFAAsync(user, tfaToken);

            if (apiResponse == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response");
            }

            if (!apiResponse.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + apiResponse.Message);
            }
            else
            {
                return Ok(apiResponse.Message);
            }

        }

        [HttpPost("two-factor authentication-enable"), Authorize]
        public async Task<ActionResult> EnableTFA()
        {

            string? userName = User.FindFirstValue(ClaimTypes.Name);
            IdentityUser? user = await _userManager.FindByNameAsync(userName);

            if (user is null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " user not found");
            }

            IdentityResult? userEnableTFA = await _userManager.SetTwoFactorEnabledAsync(user, true);

            if (userEnableTFA == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response");
            }

            if (!userEnableTFA.Succeeded)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name);
            }
            else
            {
                return Ok("2FA enabled");
            }

        }

        [HttpPost("two-factor authentication-disable"), Authorize]
        public async Task<ActionResult> DisableTFA()
        {

            string? userName = User.FindFirstValue(ClaimTypes.Name);
            IdentityUser? user = await _userManager.FindByNameAsync(userName);

            if (user is null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " user not found");
            }

            IdentityResult? userEnableTFA = await _userManager.SetTwoFactorEnabledAsync(user, false);

            if (userEnableTFA == null)
            {
                return BadRequest("Response is null");
            }

            if (!userEnableTFA.Succeeded)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name);
            }
            else
            {
                return Ok("2FA enabled");
            }
        }


        [HttpPost("change-password"), Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] string oldPassword, string newPassword)
        {
            string? userName = User.FindFirstValue(ClaimTypes.Name);

            IdentityUser? user = await _userManager.FindByNameAsync(userName);

            if (user is null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " user not found");
            }

            ApiResponse? apiResponse = await _unitOfWorkRepository.AuthRepository.ChangePasswordAsync(user, oldPassword, newPassword);

            if (apiResponse == null)
            {
                return BadRequest("Response is null");
            }

            if (!apiResponse.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + apiResponse.Message);
            }
            else
            {
                return Ok("Password has been changed successfully");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromForm] string email)
        {

            ApiResponse? apiResponse = await _unitOfWorkRepository.AuthRepository.ForgotPasswordAsync(email);

            if (apiResponse == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response ");
            }

            if (!apiResponse.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + apiResponse.Message);
            }
            else
            {
                return Ok(apiResponse.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromForm] string email, string code, string newPassword)
        {
            IdentityUser? userExists = await _userManager.FindByEmailAsync(email);

            if (userExists == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " user not Found ");
            }

            byte[]? decodedToken = WebEncoders.Base64UrlDecode(code);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            ApiResponse? apiResponse = await _unitOfWorkRepository.AuthRepository.ResetPasswordAsync(userExists, normalToken, newPassword);

            if (apiResponse == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response ");
            }

            if (!apiResponse.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + apiResponse.Message);
            }
            else
            {
                return Ok(apiResponse.Message);
            }
        }

        [HttpPost("logout-all-users")]
        public async Task<ActionResult> Logout()
        {
            ApiResponse? apiResponse = await _unitOfWorkRepository.AuthRepository.LogoutAsync();

            if (apiResponse == null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " no response ");
            }

            if (!apiResponse.Success)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + apiResponse.Message);
            }
            else
            {
                return Ok(apiResponse.Message);
            }

        }

    }
}