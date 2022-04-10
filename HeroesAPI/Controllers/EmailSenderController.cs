using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSenderController : ControllerBase
    {
        readonly IEmailSenderRepository _emailSenderRepository;

        private readonly ILogger<EmailSenderController> _logger;

        public EmailSenderController(IEmailSenderRepository emailSenderRepository, ILogger<EmailSenderController> logger)
        {
            _emailSenderRepository = emailSenderRepository;
            _logger = logger;
        }

        [HttpPost, Route("SendEmail")]
        public async Task<IActionResult> SendEmail([FromForm] EmailModel emailModel)
        {
            ApiResponse apiResponse = await _emailSenderRepository.SendEmailAsync(emailModel);

            if (apiResponse == null)
            {
                throw new ApplicationException(GetType().Name + " " + "response failed");
            }

            return Ok(apiResponse.Message);
        }

    }
}
