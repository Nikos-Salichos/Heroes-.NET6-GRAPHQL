﻿using HeroesAPI.Entitites.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
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

        public async Task<Response> Register(UserRegister userRegister)
        {
            Response registrationResponse = new Response();
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

                string code = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                string? codeHtmlVersion = HttpUtility.UrlEncode(code);

                UriBuilder? uriBuilder = new UriBuilder(identityUser.Email) { Port = -1 };
                System.Collections.Specialized.NameValueCollection? nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
                nameValueCollection["userId"] = identityUser.Id;
                nameValueCollection["code"] = codeHtmlVersion;
                uriBuilder.Query = nameValueCollection.ToString();

                WelcomeRequest welcomeRequest = new WelcomeRequest();
                welcomeRequest.ToEmail = userRegister.Email;
                welcomeRequest.UserName = userRegister.Username;
                welcomeRequest.ConfirmationCode = codeHtmlVersion;
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

        public async Task<string> Login(UserLogin userLogin)
        {
            try
            {
                IdentityUser? user = await _userManager.FindByNameAsync(userLogin.Username);

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

                    JwtSecurityToken? token = GetToken(authClaims);

                    return new JwtSecurityTokenHandler().WriteToken(token);

                }
                return "Unauthorized";
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return exception.Message;
            }
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            SymmetricSecurityKey? authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            JwtSecurityToken? token = new JwtSecurityToken(
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

        public async Task<bool> UserExists(string email)
        {
            bool userExists = await _msSql.Users.AnyAsync(user => user.Email.ToLower().Equals(email.ToLower()));

            if (!userExists)
            {
                return false;
            }
            return true;
        }

        public async Task<Response> ChangePassword(IdentityUser identityUser, string oldPassword, string newPassword)
        {
            Response registrationResponse = new Response();
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

    }
}