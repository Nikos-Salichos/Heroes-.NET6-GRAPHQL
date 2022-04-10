using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Identity;

namespace HeroesAPI.Interfaces
{
    public interface IAuthRepository
    {

        Task<ApiResponse> RegisterAsync(UserRegister userRegister);

        Task<ApiResponse> RegisterAdminAsync(UserRegister userRegister);

        Task<bool> UserExists(string email);

        Task<ApiResponse> LoginAsync(UserLogin userLogin);

        Task<ApiResponse> ForgotPasswordAsync(string email);

        Task<ApiResponse> ChangePasswordAsync(IdentityUser identityUser, string oldPassword, string newPassword);

        Task<ApiResponse> ResetPasswordAsync(IdentityUser identityUser, string code, string newPassword);

        Task<bool> SendWelcomeEmailAsync(WelcomeRequest request);

        Task<ApiResponse> LogoutAsync();

        Task<ApiResponse> ValidateTFAAsync(IdentityUser identityUser, string tFAToken);

    }
}
