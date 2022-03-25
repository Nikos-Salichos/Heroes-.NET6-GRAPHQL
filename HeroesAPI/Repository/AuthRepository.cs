using HeroesAPI.Entitites.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HeroesAPI.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly MsSql _msSql;

        private readonly IConfiguration _configuration;

        public AuthRepository(MsSql msSql, IConfiguration configuration)
        {
            _msSql = msSql;
            _configuration = configuration;
        }

        public async Task<string> Register(User user, string password)
        {

            if (await UserExists(user.Email))
            {
                return "User Exists";
            }
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = (passwordHash);
            user.PasswordSalt = (passwordSalt);

            await _msSql.Users.AddAsync(user);
            await _msSql.SaveChangesAsync();

            return "User registered successfully";
        }


        public async Task<string> Login(string email, string password)
        {
            User? user = await _msSql.Users.FirstOrDefaultAsync(u => u.Email.ToLower()
                                                                          .Equals(email.ToLower()));

            if (user is null)
            {
                return "User not found";
            }
            else if (!VerifyPasswordHash(password, (user.PasswordHash), (user.PasswordSalt)))
            {
                return "Wrong password";
            }
            else
            {
                return CreateToken(user);
            }
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
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email.ToString()),
            };

            SymmetricSecurityKey? key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            SigningCredentials? credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            JwtSecurityToken? token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: credentials);

            string? jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;

        }

        public async Task<string> ChangePassword(int userId, string newPassword)
        {
            User user = await _msSql.Users.FindAsync(userId);

            if (user is null)
            {
                return "User not found";
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _msSql.SaveChangesAsync();

            return "Password has been changed successfully";
        }
    }
}