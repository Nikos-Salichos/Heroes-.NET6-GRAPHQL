using HeroesAPI.Entities.Models;
using HeroesAPI.Interfaces;
using HeroesAPI.Paging;
using HeroesAPI.Sorting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        private readonly ILogger<HeroController> _logger;

        private readonly IHeroRepository _heroRepository;

        private readonly IMemoryCache _memoryCache;

        public HeroController(IHeroRepository heroRepository, IMemoryCache memoryCache, ILogger<HeroController> logger)
        {
            _heroRepository = heroRepository;
            _memoryCache = memoryCache;
            _logger = logger;
        }

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
                        AbsoluteExpiration = DateTime.Now.AddMinutes(2),
                        Priority = CacheItemPriority.Normal,
                        SlidingExpiration = TimeSpan.FromSeconds(30)
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
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpGet("/heroDetails", Name = "HeroById")]
        public async Task<ActionResult<Hero>> GetOneHero(int heroId)
        {
            try
            {
                Hero? hero = await _heroRepository.GetHeroByIdAsync(heroId);

                if (hero is null)
                {
                    return NotFound("Hero not found");
                }

                return Ok(hero);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpGet("/heroImage", Name = "HeroByIdImage")]
        public async Task<ActionResult<Hero>> GetHeroImage(int heroId)
        {
            try
            {
                Hero? hero = await _heroRepository.GetHeroByIdAsync(heroId);

                if (hero is null)
                {
                    return NotFound("Hero not found");
                }

                string? imageUrl = hero.ImageUrl;
                if (imageUrl != null && System.IO.File.Exists(imageUrl))
                {
                    byte[] byteArray = System.IO.File.ReadAllBytes(imageUrl);
                    return File(byteArray, "image/png");
                }

                return NotFound("Hero image not found");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddHero([FromForm] Hero newHero)
        {
            try
            {
                Hero? heroExist = await HeroAlreadyExists(newHero);

                if (heroExist is not null)
                {
                    return Conflict(new { message = "An existing record with same Name/FirstName/LastName/Place was already found." });
                }

                if (newHero.Image?.Length > 0)
                {
                    if (!newHero.Image.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                     && !newHero.Image.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                     && !newHero.Image.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                       )
                    {
                        return BadRequest(new { message = "This file is not image" });
                    }

                    string pathToSave = CreateImageDirectory();
                    SaveImageInDir(newHero, pathToSave, out string fullPath, out string extension);
                    newHero.ImageUrl = fullPath + extension;
                }

                _heroRepository.CreateHero(newHero);

                await _heroRepository.SaveAsync();

                return Ok(newHero);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        private static void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension)
        {
            string imageName = Guid.NewGuid().ToString();
            fullPath = Path.Combine(pathToSave, imageName);
            extension = Path.GetExtension(newHero.Image.FileName);
            using (FileStream fileStream = System.IO.File.Create(fullPath + imageName + extension))
            {
                newHero.Image.CopyTo(fileStream);
                fileStream.Flush();
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateHero([FromForm] Hero requestedHero)
        {
            try
            {
                Hero? hero = await _heroRepository.GetHeroByIdAsync(requestedHero.Id);

                if (hero is null)
                {
                    return BadRequest("Hero not found");
                }

                if (requestedHero.Image?.Length > 0)
                {
                    if (!requestedHero.Image.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                     && !requestedHero.Image.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                     && !requestedHero.Image.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                       )
                    {
                        return BadRequest(new { message = "This file is not image" });
                    }

                    string pathToSave = CreateImageDirectory();
                    SaveImageInDir(requestedHero, pathToSave, out string fullPath, out string extension);
                    requestedHero.ImageUrl = fullPath + extension;
                }

                _heroRepository.UpdateHero(requestedHero);
                await _heroRepository.SaveAsync();

                return Ok(requestedHero);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        private static string CreateImageDirectory()
        {
            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Resources", "Images"));
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            return pathToSave;
        }

        [HttpDelete("{heroId}")]
        public async Task<IActionResult> DeleteHero(int heroId)
        {
            try
            {
                Hero? hero = await _heroRepository.GetHeroByIdAsync(heroId);

                if (hero is null)
                {
                    return NotFound("Hero not found");
                }

                _heroRepository.DeleteHero(hero);
                await _heroRepository.SaveAsync();

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }






        private async Task<List<Hero>> GetHeroesPagination(PaginationFilter paginationFilter)
        {
            IEnumerable<Hero>? allHeroes = await _heroRepository.GetAllHeroesAsync();

            List<Hero> allHeroesByPageSizeAndNumber = allHeroes.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                                             .Take(paginationFilter.PageSize)
                                                             .ToList();
            return allHeroesByPageSizeAndNumber;
        }

        private async Task<IActionResult> HeroesWithSorting(string? searchString, string sortBy, PaginationFilter validFilter)
        {
            List<Hero> allHeroesByPageSizeAndNumber = await GetHeroesPagination(validFilter);

            List<Hero> allHeroesSortBy = allHeroesByPageSizeAndNumber.OrderByProperty(sortBy).ToList();

            if (searchString is not null)
            {
                allHeroesSortBy = HeroesFiltering(searchString, allHeroesSortBy);
            }

            return Ok(new PagedResponse<IEnumerable<Hero>>(allHeroesSortBy, validFilter.PageNumber, validFilter.PageSize));
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

        private async Task<IActionResult> HeroesWithoutSorting(string? searchString, PaginationFilter validFilter)
        {
            List<Hero> allHeroesByPageSizeAndNumber = await GetHeroesPagination(validFilter);

            allHeroesByPageSizeAndNumber = HeroesFiltering(searchString, allHeroesByPageSizeAndNumber);
            return Ok(new PagedResponse<List<Hero>>(allHeroesByPageSizeAndNumber, validFilter.PageNumber, validFilter.PageSize));
        }

        private async Task<Hero> HeroAlreadyExists(Hero newHero)
        {
            IEnumerable<Hero>? allheroes = await _heroRepository.GetAllHeroesAsync();

            Hero? heroExist = allheroes.AsEnumerable().FirstOrDefault(h => h.Name.Equals(newHero.Name, StringComparison.InvariantCultureIgnoreCase));
            return heroExist;
        }

    }
}
