using HeroesAPI.Entities.Models;
using HeroesAPI.Paging;
using HeroesAPI.Sorting;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        private readonly ILogger<HeroController> _logger;

        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        public HeroController(ILogger<HeroController> logger, IUnitOfWorkRepository unitOfWorkRepository)
        {
            _logger = logger;
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        [Route("GetAllHeroes")]
        [HttpGet]
        [ResponseCache(CacheProfileName = "60SecondsDuration")]
        public async Task<IActionResult> GetAllOwners(string? searchString, string? sortBy, [FromQuery] PaginationFilter filter)
        {
            try
            {
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

                Hero? hero = await _unitOfWorkRepository.HeroRepository.GetHeroByIdAsync(heroId);

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
                Hero? hero = await _unitOfWorkRepository.HeroRepository.GetHeroByIdAsync(heroId);

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

                _unitOfWorkRepository.HeroRepository.CreateHero(newHero);

                await _unitOfWorkRepository.HeroRepository.SaveAsync();

                return Ok(newHero);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateHero([FromForm] Hero requestedHero)
        {
            try
            {
                Hero? hero = await _unitOfWorkRepository.HeroRepository.GetHeroByIdAsync(requestedHero.Id);

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

                _unitOfWorkRepository.HeroRepository.UpdateHero(requestedHero);
                await _unitOfWorkRepository.HeroRepository.SaveAsync();

                return Ok(requestedHero);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpDelete("{heroId}")]
        public async Task<IActionResult> DeleteHero(int heroId)
        {
            try
            {
                Hero? hero = await _unitOfWorkRepository.HeroRepository.GetHeroByIdAsync(heroId);

                if (hero is null)
                {
                    return NotFound("Hero not found");
                }

                _unitOfWorkRepository.HeroRepository.DeleteHero(hero);
                await _unitOfWorkRepository.HeroRepository.SaveAsync();

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }


        #region Helper Methods
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
        private static string CreateImageDirectory()
        {
            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Resources", "Images"));
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            return pathToSave;
        }
        private async Task<List<Hero>> GetHeroesPagination(PaginationFilter paginationFilter)
        {
            IEnumerable<Hero>? allHeroes = await _unitOfWorkRepository.HeroRepository.GetAllHeroesAsync();

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
            IEnumerable<Hero>? allheroes = await _unitOfWorkRepository.HeroRepository.GetAllHeroesAsync();

            Hero? heroExist = allheroes.AsEnumerable().FirstOrDefault(h => h.Name.Equals(newHero.Name, StringComparison.InvariantCultureIgnoreCase));
            return heroExist;
        }
        #endregion Helper Methods

    }
}
