using HeroesAPI.Entitites.Models;

namespace HeroesAPI.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> Register(User user, string password);

        Task<bool> UserExists(string email);

        Task<bool> Login(string username, string password);
    }
}
