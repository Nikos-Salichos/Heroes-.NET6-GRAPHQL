using HeroesAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        private readonly ILogger<HeroController> _logger;
        private readonly DataContext _dataContext;

        public HeroController(DataContext dataContext, ILogger<HeroController> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }


        [HttpGet]
        public async Task<ActionResult<List<Hero>>> GetAllHeroes()
        {
            try
            {
                return Ok(await _dataContext.Heroes.ToListAsync());
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Hero>> GetOneHero(int id)
        {
            try
            {
                var hero = await _dataContext.Heroes.FindAsync(id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                return Ok(hero);
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }


        [HttpPost]
        public async Task<ActionResult<Hero>> AddHero(Hero newHero)
        {
            try
            {
                _dataContext.Heroes.Add(newHero);
                await _dataContext.SaveChangesAsync();

                return Ok(newHero);
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<ActionResult<Hero>> UpdateHero([FromBody] Hero requestedHero)
        {
            try
            {
                var hero = await _dataContext.Heroes.FindAsync(requestedHero.Id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                hero.Name = requestedHero.Name;
                hero.FirstName = requestedHero.FirstName;
                hero.LastName = requestedHero.LastName;
                hero.Place = requestedHero.Place;

                await _dataContext.SaveChangesAsync();

                return Ok(hero);
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteHero(int id)
        {
            try
            {
                var hero = await _dataContext.Heroes.FindAsync(id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                _dataContext.Heroes.Remove(hero);

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }

        }

    }
}
