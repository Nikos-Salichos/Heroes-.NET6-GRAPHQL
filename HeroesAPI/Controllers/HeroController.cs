using HeroesAPI.Entities.Models;
using HeroesAPI.Interfaces;
using HeroesAPI.Paging;
using HeroesAPI.Sorting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        private readonly DataContext _dataContext;

        private readonly IHeroRepository _heroRepository;

        private readonly IMemoryCache _memoryCache;

        public HeroController(IHeroRepository heroRepository, DataContext dataContext, IMemoryCache memoryCache)
        {
            _heroRepository = heroRepository;
            _dataContext = dataContext;
            _memoryCache = memoryCache;
        }

        //TODO Replace CRUD with heroRepository(Generic pattern)

        [Route("GetAllHeroes")]
        [HttpGet]
        public async Task<IActionResult> GetAllOwners(string? searchString, string? sortBy, [FromQuery] PaginationFilter filter)
        {
            try
            {

                string? cacheKey = "heroesList";
                if (!_memoryCache.TryGetValue(cacheKey, out List<Hero> customerList))
                {
                    customerList = (List<Hero>)await _heroRepository.GetAllHeroesAsync();
                    var cacheExpiryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                        Priority = CacheItemPriority.High,
                        SlidingExpiration = TimeSpan.FromMinutes(2)
                    };
                    _memoryCache.Set(cacheKey, customerList, cacheExpiryOptions);
                }

                PaginationFilter? validFilter = new(filter.PageNumber, filter.PageSize);
                if (sortBy is not null)
                {
                    return await HeroesWithSorting(searchString, sortBy, validFilter);

                }
                else
                {
                    return await HeroesWithoutSorting(searchString, validFilter);
                }
            }
            catch (Exception exception)
            {
                Serilog.Log.Information($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        private async Task<IActionResult> HeroesWithSorting(string? searchString, string sortBy, PaginationFilter validFilter)
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

        private async Task<IActionResult> HeroesWithoutSorting(string? searchString, PaginationFilter validFilter)
        {
            List<Hero> allHeroesByPageSizeAndNumber = await _dataContext.Heroes.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                             .Take(validFilter.PageSize)
                                                             .ToListAsync();

            allHeroesByPageSizeAndNumber = HeroesFiltering(searchString, allHeroesByPageSizeAndNumber);
            return Ok(new PagedResponse<List<Hero>>(allHeroesByPageSizeAndNumber, validFilter.PageNumber, validFilter.PageSize));
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

        [HttpGet("{heroId}", Name = "HeroById")]
        public async Task<ActionResult<Hero>> GetOneHero(int heroId)
        {
            try
            {
                Hero? hero = await _heroRepository.GetHeroByIdAsync(heroId);

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
        public async Task<IActionResult> AddHero(Hero newHero)
        {
            try
            {
                IEnumerable<Hero>? allheroes = await _heroRepository.GetAllHeroesAsync();

                Hero? heroExist = allheroes.AsEnumerable().FirstOrDefault(h => h.Name == newHero.Name
                                                                 && h.FirstName == newHero.FirstName
                                                                 && h.LastName == newHero.LastName
                                                                 && h.Place == newHero.Place);

                if (heroExist is not null)
                {
                    return Conflict(new { message = "An existing record with same Name/FirstName/LastName/Place was already found." });
                }

                _heroRepository.CreateHero(newHero);

                await _heroRepository.SaveAsync();

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
