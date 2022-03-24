using HeroesAPI.Entitites.Models;
using System.Security.Cryptography;

namespace HeroesAPI.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly MsSql _msSql;

        public AuthRepository(MsSql msSql)
        {
            _msSql = msSql;
        }

        public async Task<bool> Login(string username, string password)
        {
            var data = "token";

            return true;
        }

        public async Task<bool> Register(User user, string password)
        {
            if (await UserExists(user.Email))
            {
                return false;
            }
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _msSql.Users.Add(user);
            _msSql.SaveChanges();

            return true;
        }

        public async Task<bool> UserExists(string email)
        {
            bool userExists = await _msSql.Users.AnyAsync(user => user.Email.ToLower().Equals(email.ToLower()));
            if (userExists)
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordHash = hmac.Key;
                passwordSalt = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

    }
}