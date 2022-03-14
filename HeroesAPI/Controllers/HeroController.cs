using HeroesAPI.Models;
using HeroesAPI.Paging;
using HeroesAPI.Sorting;
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
        public async Task<ActionResult<List<Hero>>> GetAllHeroes(string? sortBy, [FromQuery] PaginationFilter filter)
        {
            try
            {
                if (sortBy is not null)
                {
                    PaginationFilter? validFilter = new(filter.PageNumber, filter.PageSize);
                    List<Hero>? allHeroes = await _dataContext.Heroes.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                                     .Take(validFilter.PageSize)
                                                                     .ToListAsync();
                    IEnumerable<Hero>? allHeroesSortBy = allHeroes.OrderByProperty(sortBy);
                    return Ok(new PagedResponse<IEnumerable<Hero>>(allHeroesSortBy, validFilter.PageNumber, validFilter.PageSize));
                }
                else
                {
                    PaginationFilter? validFilter = new(filter.PageNumber, filter.PageSize);
                    List<Hero>? allHeroes = await _dataContext.Heroes.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                                     .Take(validFilter.PageSize)
                                                                     .ToListAsync();
                    return Ok(new PagedResponse<List<Hero>>(allHeroes, validFilter.PageNumber, validFilter.PageSize));
                }
            }
            catch (Exception exception)
            {
                Serilog.Log.Information($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
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
                Serilog.Log.Information($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
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
                Serilog.Log.Information($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
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
                Serilog.Log.Information($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
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
                Serilog.Log.Information($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }

        }

    }
}
