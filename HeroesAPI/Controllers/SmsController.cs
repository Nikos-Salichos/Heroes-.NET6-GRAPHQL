using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HeroesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly ITwilioRestClient _client;

        private readonly ILogger<SmsController> _logger;

        public SmsController(ILogger<SmsController> logger, ITwilioRestClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet]
        [Route("SendSms")]
        public IActionResult SendSms([FromQuery] SmsMessage model)
        {
            try
            {
                MessageResource? message = MessageResource.Create(
                    from: new PhoneNumber(model.From),
                    to: new PhoneNumber(model.To),
                    body: model.Message,
                    client: _client); // pass in the custom client

                if (message.From is null || message.To is null || message.Body is null)
                {
                    return BadRequest("You must fill all fields");
                }

                return Ok("Success");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return BadRequest();
            }
        }

    }
}
