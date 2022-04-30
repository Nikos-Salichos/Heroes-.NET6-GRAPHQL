using HeroesAPI.Models;
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
        private readonly ITwilioRestClient _twilioRestClient;

        private readonly ILogger<SmsController> _logger;

        public SmsController(ILogger<SmsController> logger, ITwilioRestClient twilioRestClient)
        {
            _logger = logger;
            _twilioRestClient = twilioRestClient;
        }

        [HttpGet]
        [Route("SendSms")]
        public IActionResult SendSms([FromQuery] SmsMessage model)
        {

            MessageResource? message = MessageResource.Create(
                from: new PhoneNumber(model.From),
                to: new PhoneNumber(model.To),
                body: model.Message,
                client: _twilioRestClient);

            if (message == null)
            {
                throw new ApplicationException(MethodBase.GetCurrentMethod() + " " + GetType().Name + " message failed");
            }

            if (message.From == null || message.To == null || message.Body == null)
            {
                throw new ApplicationException(MethodBase.GetCurrentMethod() + " " + GetType().Name + " you must fill all fields");
            }

            return Ok("Success");

        }

    }
}
