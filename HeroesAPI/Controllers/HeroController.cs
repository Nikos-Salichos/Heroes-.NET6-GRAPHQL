using HeroesAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {

        private static List<Hero> heroes = new List<Hero>
        {
            new Hero
            {
                Id = 1,
                Name = "Spider Man",
                FirstName = "Peter",
                LastName = "Parker",
                Place = "New York"
            }
        };



        [HttpGet]
        public async Task<ActionResult<List<Hero>>> GetAllHeroes()
        {
            try
            {
                return Ok(heroes);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }


        [HttpPost]
        public async Task<ActionResult<List<Hero>>> AddHero(Hero newHero)
        {
            heroes.Add(newHero);
            return Ok(newHero);
        }

    }
}
