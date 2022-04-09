using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Identity;

namespace HeroesAPI.Interfaces
{
    public interface IAuthRepository
    {

        Task<Response> RegisterAsync(UserRegister userRegister);

        Task<Response> RegisterAdminAsync(UserRegister userRegister);

        Task<bool> UserExists(string email);

        Task<Response> LoginAsync(UserLogin userLogin);

        Task<Response> ForgotPasswordAsync(string email);

        Task<Response> ChangePasswordAsync(IdentityUser identityUser, string oldPassword, string newPassword);

        Task<Response> ResetPasswordAsync(IdentityUser identityUser, string code, string newPassword);

        Task<bool> SendWelcomeEmailAsync(WelcomeRequest request);

        Task<Response> LogoutAsync();

        Task<Response> ValidateTFAAsync(IdentityUser identityUser, string tFAToken);

    }
}
