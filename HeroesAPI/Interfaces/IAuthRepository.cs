using HeroesAPI.Entitites.Models;

namespace HeroesAPI.Interfaces
{
    public interface IAuthRepository
    {
        Task<string> Register(User user, string password);

        Task<bool> UserExists(string email);

        Task<string> Login(string email, string password);

        Task<string> ChangePassword(int userId, string newPassword);

        Task<bool> SendWelcomeEmailAsync(WelcomeRequest request);
    }
}
