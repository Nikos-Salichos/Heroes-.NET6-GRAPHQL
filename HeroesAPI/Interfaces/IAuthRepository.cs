using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Identity;

namespace HeroesAPI.Interfaces
{
    public interface IAuthRepository
    {
        Task<Response> Register(UserRegister userRegister);

        Task<Response> RegisterAdmin(UserRegister userRegister);

        Task<bool> UserExists(string email);

        Task<string> Login(UserLogin userLogin);

        Task<Response> ChangePassword(IdentityUser identityUser, string oldPassword, string newPassword);

        Task<bool> SendWelcomeEmailAsync(WelcomeRequest request);
    }
}
