using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromForm] UserRegister userRegister)
        {
            if (userRegister is null)
            {
                return NotFound();
            }

            var response = await _authRepository.Register(new User
            {
                Email = userRegister.Email
            },
            userRegister.Password);


            if (!response)
            {
                return BadRequest("User already exists");
            }

            return Ok(userRegister);
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLogin userLogin)
        {
            var response = await _authRepository.Login(userLogin.Email, userLogin.Password);

            if (!response)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //todo 20

    }
}