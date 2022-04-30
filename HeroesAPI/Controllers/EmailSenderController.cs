using HeroesAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSenderController : ControllerBase
    {

        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        private readonly ILogger<EmailSenderController> _logger;

        public EmailSenderController(IUnitOfWorkRepository unitOfWorkRepository, ILogger<EmailSenderController> logger)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
            _logger = logger;
        }

        [HttpPost, Route("SendEmail")]
        public async Task<IActionResult> SendEmail([FromForm] EmailModel emailModel)
        {
            ApiResponse apiResponse = await _unitOfWorkRepository.EmailSenderRepository.SendEmailAsync(emailModel);

            if (apiResponse == null)
            {
                throw new ApplicationException(GetType().Name + " " + "response failed");
            }

            return Ok(apiResponse.Message);
        }

    }
}
