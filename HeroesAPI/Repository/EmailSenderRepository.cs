using HeroesAPI.Entitites.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net;
using System.Reflection;

namespace HeroesAPI.Repository
{
    public class EmailSenderRepository : IEmailSenderRepository
    {

        private readonly SmtpSettings _smptSettings = new SmtpSettings();

        private readonly ILogger<EmailSenderRepository> _logger;

        public EmailSenderRepository(IConfiguration smptSettings, ILogger<EmailSenderRepository> logger)
        {
            _smptSettings.Server = smptSettings["SmtpSettings:Server"];
            _smptSettings.Port = Convert.ToInt32(smptSettings["SmtpSettings:Port"]);
            _smptSettings.SenderMail = smptSettings["SmtpSettings:SenderMail"];
            _smptSettings.Password = smptSettings["SmtpSettings:Password"];
            _logger = logger;
        }

        public async Task<ApiResponse> SendEmailAsync(EmailModel emailModel)
        {
            SmtpClient smtpClient = new SmtpClient();

            ApiResponse errorResponse = new ApiResponse();
            try
            {
                MimeMessage mimeMessage = new MimeMessage();
                mimeMessage.From.Add(MailboxAddress.Parse(_smptSettings.SenderMail));
                mimeMessage.To.Add(MailboxAddress.Parse(emailModel.RecipientEmail));
                mimeMessage.Subject = emailModel.Subject;

                BodyBuilder builder = new BodyBuilder();
                if (emailModel.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (var file in emailModel.Attachments.Where(file => file.Length > 0))
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }

                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
                builder.HtmlBody = emailModel.Body;
                mimeMessage.Body = builder.ToMessageBody();

                await smtpClient.ConnectAsync(_smptSettings.Server, _smptSettings.Port, false);
                await smtpClient.AuthenticateAsync(new NetworkCredential(_smptSettings.SenderMail, _smptSettings.Password));
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);

                errorResponse.Success = true;
                errorResponse.Message.Add("Email send Successfully!");

                return errorResponse;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);

                errorResponse.Success = false;
                errorResponse.Message.Add($"{MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return errorResponse;
            }
            finally
            {
                smtpClient.Dispose();
            }

        }
    }
}
