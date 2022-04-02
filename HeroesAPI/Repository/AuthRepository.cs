using HeroesAPI.Entitites.Models;
using MailKit.Net.Smtp;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HeroesAPI.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly MsSql _msSql;

        private readonly IConfiguration _configuration;

        private readonly ILogger<EmailSenderRepository> _logger;

        public AuthRepository(MsSql msSql, IConfiguration configuration, ILogger<EmailSenderRepository> logger)
        {
            _msSql = msSql;
            _configuration = configuration;
            _logger = logger;
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

            WelcomeRequest welcomeRequest = new WelcomeRequest();
            welcomeRequest.ToEmail = user.Email;
            welcomeRequest.UserName = user.Username;

            bool emailSent = await SendWelcomeEmailAsync(welcomeRequest);

            if (emailSent)
            {
                return "User registered successfully but welcome email failed";
            }
            else
            {
                return "User registered successfully and welcome email sent";
            }
        }


        public async Task<string> Login(string email, string password)
        {
            User? user = await _msSql.Users.FirstOrDefaultAsync(u => u.Email.ToLower()
                                                                          .Equals(email.ToLower()));

            if (user is null)
            {
                return "User not found";
            }
            else if (user.IsEmailConfirmed == 0)
            {
                return "User is not confirmed";
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

        public async Task<bool> SendWelcomeEmailAsync(WelcomeRequest request)
        {

            try
            {
                MimeMessage mimeMessage = new MimeMessage();

                var senderEmail = _configuration.GetSection("SmtpSettings:SenderMail").Value;
                mimeMessage.From.Add(MailboxAddress.Parse(senderEmail));

                var toEmail = request.ToEmail;
                mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
                mimeMessage.Subject = "Welcome email!";

                BodyBuilder builder = new BodyBuilder();
                builder.HtmlBody = $"Dear {request.UserName} \r\n Welcome to heroes API";
                mimeMessage.Body = builder.ToMessageBody();

                SmtpClient smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(_configuration.GetSection("SmtpSettings:Server").Value, Convert.ToInt32(_configuration.GetSection("SmtpSettings:Port").Value), false);
                await smtpClient.AuthenticateAsync(new NetworkCredential(_configuration.GetSection("SmtpSettings:SenderMail").Value, _configuration.GetSection("SmtpSettings:Password").Value));
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return false;
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
                new Claim(ClaimTypes.Role, "Admin" )
            };

            SymmetricSecurityKey? key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            SigningCredentials? credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            JwtSecurityToken? token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );

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