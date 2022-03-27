using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;
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
        public SmsController(ITwilioRestClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IActionResult SendSms([FromQuery] SmsMessage model)
        {
            MessageResource? message = MessageResource.Create(
                from: new PhoneNumber(model.From),
                to: new PhoneNumber(model.To),
                body: model.Message,
                client: _client); // pass in the custom client

            return Ok("Success");
        }

    }
}
