using HeroesAPI.Entities.Models;
using HeroesAPI.Interfaces;
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
        private readonly DataContext _dataContext;

        private readonly IHeroRepository _heroRepository;

        public HeroController(IHeroRepository heroRepository, DataContext dataContext)
        {
            _heroRepository = heroRepository;
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOwners(string? searchString, string? sortBy, [FromQuery] PaginationFilter filter)
        {
            try
            {
                PaginationFilter? validFilter = new(filter.PageNumber, filter.PageSize);
                if (sortBy is not null)
                {

                    IEnumerable<Hero>? allHeroes = await _heroRepository.GetAllHeroesAsync();

                    List<Hero> allHeroesByPageSizeAndNumber = allHeroes.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                                       .Take(validFilter.PageSize)
                                                                       .ToList();

                    List<Hero> allHeroesSortBy = allHeroesByPageSizeAndNumber.OrderByProperty(sortBy).ToList();

                    if (searchString is not null)
                    {
                        allHeroesSortBy = HeroesFiltering(searchString, allHeroesSortBy);
                    }

                    return Ok(new PagedResponse<IEnumerable<Hero>>(allHeroesSortBy, validFilter.PageNumber, validFilter.PageSize));

                }
                else
                {
                    List<Hero> allHeroes = await _dataContext.Heroes.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                                     .Take(validFilter.PageSize)
                                                                     .ToListAsync();

                    allHeroes = HeroesFiltering(searchString, allHeroes);
                    return Ok(new PagedResponse<List<Hero>>(allHeroes, validFilter.PageNumber, validFilter.PageSize));
                }
            }
            catch (Exception exception)
            {
                Serilog.Log.Information($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        private static List<Hero> HeroesFiltering(string? searchString, List<Hero> allHeroes)
        {
            if (searchString is not null)
            {
                allHeroes = allHeroes.Where(h => h.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                                                 .ToList();
            }

            return allHeroes;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Hero>> GetOneHero(int id)
        {
            try
            {
                Hero? hero = await _dataContext.Heroes.FindAsync(id);

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
                Task<Hero?>? heroExist = _dataContext.Heroes.FirstOrDefaultAsync(h => h.Name == newHero.Name
                                                                                 && h.FirstName == newHero.FirstName
                                                                                 && h.LastName == newHero.LastName
                                                                                 && h.Place == newHero.Place);
                if (heroExist is null)
                {
                    return Conflict(new { message = "An existing record with same Name/FirstName/LastName/Place was already found." });
                }

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
                Hero? hero = await _dataContext.Heroes.FindAsync(requestedHero.Id);

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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteHero(int id)
        {
            try
            {
                Hero? hero = await _dataContext.Heroes.FindAsync(id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                _dataContext.Heroes.Remove(hero);
                await _dataContext.SaveChangesAsync();

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
