using HeroesAPI.Entitites.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace HeroesAPI.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly MsSql _msSql;

        private readonly IConfiguration _configuration;

        private readonly ILogger<EmailSenderRepository> _logger;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;


        public AuthRepository(MsSql msSql, IConfiguration configuration, ILogger<EmailSenderRepository> logger,
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _msSql = msSql;
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<ApiResponse> RegisterAsync(UserRegister userRegister)
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                if (await UserExists(userRegister.Email))
                {
                    registrationResponse.Status = "999";
                    registrationResponse.Message.Add("User Exists");
                    return registrationResponse;
                }

                IdentityUser identityUser = new()
                {
                    Email = userRegister.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = userRegister.Username
                };

                IdentityResult? result = await _userManager.CreateAsync(identityUser, userRegister.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        registrationResponse.Status = "999";
                        registrationResponse.Message.Add(error);
                    }
                    return registrationResponse;
                }

                string emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

                byte[] encodedConfirmationToken = Encoding.UTF8.GetBytes(emailConfirmationToken);
                string emailToken = WebEncoders.Base64UrlEncode(encodedConfirmationToken);
                UriBuilder? uriBuilder = new UriBuilder(identityUser.Email) { Port = -1 };
                System.Collections.Specialized.NameValueCollection? nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
                nameValueCollection["userId"] = identityUser.Id;
                nameValueCollection["code"] = emailToken;
                uriBuilder.Query = nameValueCollection.ToString();

                WelcomeRequest welcomeRequest = new WelcomeRequest();
                welcomeRequest.ToEmail = userRegister.Email;
                welcomeRequest.UserName = userRegister.Username;
                welcomeRequest.ConfirmationCode = emailToken;
                welcomeRequest.UriBuilder = uriBuilder;

                bool emailSent = await SendWelcomeEmailAsync(welcomeRequest);

                if (emailSent)
                {
                    registrationResponse.Status = "200";
                    registrationResponse.Message.Add("User registered successfully and welcome email sent");
                    return registrationResponse;
                }
                else
                {
                    registrationResponse.Status = "200";
                    registrationResponse.Message.Add("User registered successfully but welcome email failed");
                    return registrationResponse;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }

        public async Task<ApiResponse> RegisterAdminAsync(UserRegister userRegister)
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                var userExists = await _userManager.FindByNameAsync(userRegister.Username);

                if (userExists != null)
                {
                    registrationResponse.Status = "999";
                    registrationResponse.Message.Add("User Exists");
                    return registrationResponse;
                }

                IdentityUser user = new()
                {
                    Email = userRegister.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = userRegister.Username
                };

                IdentityResult? result = await _userManager.CreateAsync(user, userRegister.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        registrationResponse.Status = "999";
                        registrationResponse.Message.Add(error);
                    }
                    return registrationResponse;
                }

                if (!await _roleManager.RoleExistsAsync(UserRole.Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRole.Admin));
                }

                if (!await _roleManager.RoleExistsAsync(UserRole.User))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRole.User));
                }

                if (await _roleManager.RoleExistsAsync(UserRole.Admin))
                {
                    await _userManager.AddToRoleAsync(user, UserRole.Admin);
                }

                if (await _roleManager.RoleExistsAsync(UserRole.Admin))
                {
                    await _userManager.AddToRoleAsync(user, UserRole.User);
                }

                registrationResponse.Status = "200";
                registrationResponse.Message.Add("User registered successfully as Admin");
                return registrationResponse;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }

        public async Task<ApiResponse> LoginAsync(UserLogin userLogin)
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                IdentityUser? user = await _userManager.FindByNameAsync(userLogin.Username);

                if (user == null)
                {
                    registrationResponse.Status = "999";
                    registrationResponse.Message.Add("User do not exist");
                    return registrationResponse;
                }

                if (user.TwoFactorEnabled)
                {
                    IList<string>? providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
                    string? mfaToken = await _userManager.GenerateTwoFactorTokenAsync(user, providers.FirstOrDefault());
                    byte[] encodedConfirmationToken = Encoding.UTF8.GetBytes(mfaToken);
                    string emailMFAToken = WebEncoders.Base64UrlEncode(encodedConfirmationToken);

                    bool emailSent = await SendTwoFACodeEmail(user, emailMFAToken);
                    if (!emailSent)
                    {
                        registrationResponse.Status = "999";
                        registrationResponse.Message.Add("2FA code mail failed");
                        return registrationResponse;
                    }
                    else
                    {
                        registrationResponse.Status = "200";
                        registrationResponse.Message.Add("2FA code mail sent");
                        return registrationResponse;
                    }
                }

                if (await _signInManager.CanSignInAsync(user))
                {

                    IList<string>? userRoles = await _userManager.GetRolesAsync(user);

                    List<Claim>? authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.Email, user.Email),
                           new Claim(ClaimTypes.NameIdentifier, user.Id),
                        };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    JwtSecurityToken? jwtSecurityToken = GetToken(authClaims);

                    var lala = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                    registrationResponse.Status = "200";
                    registrationResponse.Message.Add(lala);
                    return registrationResponse;
                }
                else
                {
                    registrationResponse.Status = "999";
                    registrationResponse.Message.Add("Unauthorized");
                    return registrationResponse;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }

        public async Task<ApiResponse> ValidateTFAAsync(IdentityUser identityUser, string tFAToken)
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                IList<string>? providers = await _userManager.GetValidTwoFactorProvidersAsync(identityUser);

                byte[]? decodedToken = WebEncoders.Base64UrlDecode(tFAToken);
                string decodedTwoMFAToken = Encoding.UTF8.GetString(decodedToken);

                bool result = await _userManager.VerifyTwoFactorTokenAsync(identityUser, providers.FirstOrDefault(), decodedTwoMFAToken);

                if (!result)
                {
                    registrationResponse.Status = "999";
                    registrationResponse.Message.Add("2FA code is wrong");
                    return registrationResponse;
                }
                else
                {
                    if (await _signInManager.CanSignInAsync(identityUser))
                    {

                        IList<string>? userRoles = await _userManager.GetRolesAsync(identityUser);

                        List<Claim>? authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, identityUser.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.Email, identityUser.Email),
                           new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
                        };

                        foreach (var userRole in userRoles)
                        {
                            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                        }

                        JwtSecurityToken? jwtSecurityToken = GetToken(authClaims);

                        var lala = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                        registrationResponse.Status = "200";
                        registrationResponse.Message.Add(lala);
                        return registrationResponse;
                    }
                    else
                    {
                        registrationResponse.Status = "999";
                        registrationResponse.Message.Add("Unauthorized");
                        return registrationResponse;
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
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
                builder.HtmlBody = $"Dear {request.UserName} \r\n Welcome to heroes API \r\n please confirm your account here <a href='{request.UriBuilder}'>link</a>";
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

        public async Task<bool> SendTwoFACodeEmail(IdentityUser identityUser, string mfaToken)
        {
            try
            {
                MimeMessage mimeMessage = new MimeMessage();

                var senderEmail = _configuration.GetSection("SmtpSettings:SenderMail").Value;
                mimeMessage.From.Add(MailboxAddress.Parse(senderEmail));

                var toEmail = identityUser.Email;
                mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
                mimeMessage.Subject = "2FA email!";

                BodyBuilder builder = new BodyBuilder();
                builder.HtmlBody = $"Dear {identityUser.UserName} your 2FA token {mfaToken}";
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

            if (!userExists)
            {
                return false;
            }
            return true;
        }

        public async Task<ApiResponse> ChangePasswordAsync(IdentityUser identityUser, string oldPassword, string newPassword)
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                IdentityResult result = await _userManager.ChangePasswordAsync(identityUser, oldPassword, newPassword);

                if (result.Succeeded)
                {
                    registrationResponse.Status = "200";
                    return registrationResponse;
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        registrationResponse.Message.Add(error);
                    }
                    registrationResponse.Status = "999";
                    return registrationResponse;
                }

            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(IdentityUser identityUser, string code, string newPassword)
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                IdentityResult result = await _userManager.ResetPasswordAsync(identityUser, code, newPassword);

                if (result.Succeeded)
                {
                    registrationResponse.Status = "200";
                    return registrationResponse;
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        registrationResponse.Message.Add(error);
                    }
                    registrationResponse.Status = "999";
                    return registrationResponse;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }

        public async Task<ApiResponse> ForgotPasswordAsync(string email)
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                IdentityUser? userExists = await _userManager.FindByEmailAsync(email);

                if (userExists == null)
                {
                    registrationResponse.Status = "999";
                    registrationResponse.Message.Add("User Exists");
                    return registrationResponse;
                }

                string? passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(userExists);

                byte[] encodedConfirmationToken = Encoding.UTF8.GetBytes(passwordResetToken);
                string emailToken = WebEncoders.Base64UrlEncode(encodedConfirmationToken);

                UriBuilder? uriBuilder = new UriBuilder(userExists.Email) { Port = -1 };
                System.Collections.Specialized.NameValueCollection? nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
                nameValueCollection["userId"] = userExists.Id;
                nameValueCollection["code"] = emailToken;
                uriBuilder.Query = nameValueCollection.ToString();

                bool emailSent = await SendForgotPasswordEmail(userExists, uriBuilder);

                if (emailSent)
                {
                    registrationResponse.Status = "200";
                    registrationResponse.Message.Add("Email send successfully");
                    return registrationResponse;
                }
                else
                {
                    registrationResponse.Status = "200";
                    registrationResponse.Message.Add("Email failed");
                    return registrationResponse;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }

        public async Task<bool> SendForgotPasswordEmail(IdentityUser identityUser, UriBuilder uriBuilder)
        {
            try
            {
                MimeMessage mimeMessage = new MimeMessage();

                string? senderEmail = _configuration.GetSection("SmtpSettings:SenderMail").Value;
                mimeMessage.From.Add(MailboxAddress.Parse(senderEmail));

                string? toEmail = identityUser.Email;
                mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
                mimeMessage.Subject = "Password reset email";

                BodyBuilder builder = new BodyBuilder();
                builder.HtmlBody = $"Dear {identityUser.UserName} \r\n Welcome to heroes API \r\n please confirm your account here <a href='{uriBuilder.Query}'>link</a>";
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

        public async Task<ApiResponse> LogoutAsync()
        {
            ApiResponse registrationResponse = new ApiResponse();
            try
            {
                await _signInManager.SignOutAsync();

                registrationResponse.Status = "200";
                registrationResponse.Message.Add("All users logged out successfully");

                return registrationResponse;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                registrationResponse.Status = "999";
                registrationResponse.Message.Add(exception.Message);
                return registrationResponse;
            }
        }


    }
}