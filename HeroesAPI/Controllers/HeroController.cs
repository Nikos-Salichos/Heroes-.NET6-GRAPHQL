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

        [HttpGet("{id}")]
        public async Task<ActionResult<List<Hero>>> GetOneHero(int id)
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
                return BadRequest(exception.Message);
            }
        }


        [HttpPost]
        public async Task<ActionResult<List<Hero>>> AddHero(Hero newHero)
        {
            heroes.Add(newHero);
            return Ok(newHero);
        }

        [HttpPut]
        public async Task<ActionResult<Hero>> UpdateHero(Hero requestedHero)
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

        [HttpDelete]
        public async Task<ActionResult> DeleteHero(int id)
        {
            var hero = heroes.FirstOrDefault(h => h.Id == requestedHero.Id);

            if (hero is null)
            {
                return BadRequest("Hero not found");
            }

            heroes.Remove(hero);

            return Ok();

        }

    }
}
