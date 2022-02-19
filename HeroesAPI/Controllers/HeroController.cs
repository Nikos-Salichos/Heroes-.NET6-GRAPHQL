using HeroesAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        private readonly ILogger<HeroController> _logger;
        public HeroController(ILogger<HeroController> logger)
        {
            _logger = logger;
        }

        private static List<Hero> heroes = new()
        {
            new Hero
            {
                Id = 1,
                Name = "Spider Man",
                FirstName = "Peter",
                LastName = "Parker",
                Place = "New York"
            },

            new Hero
            {
                Id = 2,
                Name = "Ironman",
                FirstName = "Tony",
                LastName = "Stark",
                Place = "Las Vegas"
            }
        };





        [HttpGet]
        public ActionResult<List<Hero>> GetAllHeroes()
        {
            try
            {
                return Ok(heroes);
            }
            catch (Exception exception)
            {
                _logger.LogInformation("Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Hero> GetOneHero(int id)
        {
            try
            {
                var hero = heroes.FirstOrDefault(h => h.Id == id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                return Ok(hero);
            }
            catch (Exception exception)
            {
                _logger.LogInformation("Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }


        [HttpPost]
        public ActionResult<Hero> AddHero(Hero newHero)
        {
            try
            {
                heroes.Add(newHero);
                return Ok(newHero);
            }
            catch (Exception exception)
            {
                _logger.LogInformation("Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpPut]
        public ActionResult<Hero> UpdateHero([FromBody] Hero requestedHero)
        {
            try
            {
                var hero = heroes.FirstOrDefault(h => h.Id == requestedHero.Id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                hero.Name = requestedHero.Name;
                hero.FirstName = requestedHero.FirstName;
                hero.LastName = requestedHero.LastName;
                hero.Place = requestedHero.Place;

                return Ok(hero);
            }
            catch (Exception exception)
            {
                _logger.LogInformation("Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpDelete]
        public ActionResult DeleteHero(int id)
        {
            try
            {
                var hero = heroes.FirstOrDefault(h => h.Id == id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                heroes.Remove(hero);

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogInformation("Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }

        }

    }
}
