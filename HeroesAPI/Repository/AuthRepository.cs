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

        public async Task<string> Login(string email, string password)
        {
            var user = await _msSql.Users.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower(), StringComparison.InvariantCultureIgnoreCase);

            if (user is null)
            {
                return "User not found;
            }
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalkt))
            {
                return "Wrong password";
            }
            else
            {
                return "token";
            }
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

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return hash.SequenceEqual(passwordHash);
            }
        }

    }
}