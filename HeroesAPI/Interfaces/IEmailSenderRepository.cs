using HeroesAPI.Entitites.Models;

namespace HeroesAPI.Interfaces
{
    public interface IEmailSenderRepository
    {
        Task<ApiResponse> SendEmailAsync(EmailModel emailModel);
    }
}
