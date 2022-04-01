using HeroesAPI.Entitites.Models;

namespace HeroesAPI.Interfaces
{
    public interface IEmailSenderRepository
    {
        Task<string> SendEmailAsync(EmailModel emailModel);
    }
}
