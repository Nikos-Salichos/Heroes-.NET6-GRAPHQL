using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

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
            try
            {
                ApiResponse errorResponse = await _emailSenderRepository.SendEmailAsync(emailModel);
                return Ok(errorResponse.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }
    }
}
