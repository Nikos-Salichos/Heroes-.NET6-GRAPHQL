using AutoMapper;
using HeroesAPI.DTOs;
using HeroesAPI.Models;
using HeroesAPI.Paging;
using HeroesAPI.Sorting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Management;
using System.Text;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        private readonly ILogger<HeroController> _logger;

        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        private readonly IMapper _mapper;

        public HeroController(ILogger<HeroController> logger, IUnitOfWorkRepository unitOfWorkRepository, IMapper mapper)
        {
            _logger = logger;
            _unitOfWorkRepository = unitOfWorkRepository;
            _mapper = mapper;
        }

        [HttpGet("AllHeroes")]
        [ResponseCache(CacheProfileName = "60SecondsDuration")]
        public async Task<ActionResult<IEnumerable<HeroDTO>>> GetAllHeroes(string? searchString, string? sortBy, [FromQuery] PaginationFilter filter)
        {
            try
            {
                //Create hardware id and validate license
                /*              string hwid = CreateSerialFromHWID();
                                string HWIDList = await ValidateSerial(hwid);

                                if (HWIDList.Contains(hwid))
                                {
                                    return BadRequest();
                                }*/

                PaginationFilter? validFilter = new(filter.PageNumber, filter.PageSize);

                if (sortBy is not null)
                {
                    List<Hero>? heroes = await HeroesWithSorting(searchString, sortBy, validFilter);
                    return Ok(_mapper.Map<IEnumerable<HeroDTO>>(heroes));
                }
                else
                {
                    List<Hero>? heroes = await HeroesWithoutSorting(searchString, validFilter);
                    return Ok(_mapper.Map<IEnumerable<HeroDTO>>(heroes));
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {_unitOfWorkRepository.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<string> ValidateSerial(string hwid)
        {
            HttpClient httpClient = new HttpClient();
            string? json = JsonConvert.SerializeObject(hwid);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            string? url = "https://urlofyoursite.com"; //valid site that you keep license
            HttpResponseMessage? response = await httpClient.PostAsync(url, data);
            response.EnsureSuccessStatusCode();

            string? HWIDList = await httpClient.GetStringAsync(url); //Website that you have license for specific website
            return HWIDList;
        }

        private static string CreateSerialFromHWID()
        {
            byte[] byteArray;
            byte[] hashedByteArray;
            StringBuilder stringBuilder = new StringBuilder();

            ManagementObjectSearcher processor = new ManagementObjectSearcher("select * from win32_processor");
            ManagementObjectCollection processorList = processor.Get();
            foreach (ManagementBaseObject? managObj in processorList)
            {
                stringBuilder.Append(managObj.Properties["processorID"].Value.ToString());
                break;
            }

            ManagementObjectSearcher bios = new ManagementObjectSearcher("select * from win32_bios");
            ManagementObjectCollection biosList = bios.Get();
            foreach (ManagementBaseObject? managObj in biosList)
            {
                stringBuilder.Append(managObj.Properties["version"].Value.ToString());
                break;
            }

            byteArray = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            hashedByteArray = System.Security.Cryptography.SHA256.Create().ComputeHash(byteArray);
            string? hwid = Convert.ToBase64String(hashedByteArray);
            return hwid;
        }

        [HttpGet("{heroId}")]
        public async Task<ActionResult<Hero>> GetOneHero(int heroId)
        {
            try
            {

                Hero? hero = await _unitOfWorkRepository.HeroRepository.GetHeroByIdAsync(heroId);

                if (hero is null)
                {
                    return NotFound("Hero not found");
                }

                return Ok(_mapper.Map<Hero>(hero));
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {_unitOfWorkRepository.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("HeroImage/{heroId}")]
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
                _logger.LogError($"Logging {_unitOfWorkRepository.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        // [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> AddHero([FromForm] Hero newHero)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                IEnumerable<Hero>? allheroes = await _unitOfWorkRepository.HeroRepository.GetAllHeroesAsync();
                Hero? heroExist = allheroes.AsEnumerable().FirstOrDefault(h => h.Name.Equals(newHero.Name, StringComparison.InvariantCultureIgnoreCase));

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

                    string pathToSave = _unitOfWorkRepository.HeroRepository.CreateImageDirectory();
                    _unitOfWorkRepository.HeroRepository.SaveImageInDir(newHero, pathToSave, out string fullPath, out string extension);
                    newHero.ImageUrl = fullPath + extension;

                }

                _unitOfWorkRepository.HeroRepository.CreateHero(newHero);

                await _unitOfWorkRepository.HeroRepository.SaveAsync();

                return Ok(newHero);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {_unitOfWorkRepository.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("ChangeHeroImage/{id:int}")]
        [Authorize(Roles = UserRole.Admin)]
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

                    string pathToSave = _unitOfWorkRepository.HeroRepository.CreateImageDirectory();
                    _unitOfWorkRepository.HeroRepository.SaveImageInDir(requestedHero, pathToSave, out string fullPath, out string extension);
                    requestedHero.ImageUrl = fullPath + extension;
                }

                _unitOfWorkRepository.HeroRepository.UpdateHero(requestedHero);
                await _unitOfWorkRepository.HeroRepository.SaveAsync();

                return Ok(requestedHero);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {_unitOfWorkRepository.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("DeleteHeroImage/{id:int}")]
        [Authorize(Roles = UserRole.Admin)]
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
                _logger.LogError($"Logging {_unitOfWorkRepository.GetCurrentMethod()} {GetType().Name}" + exception.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        #region Helper Methods



        private async Task<List<Hero>> GetHeroesPagination(PaginationFilter paginationFilter)
        {
            IEnumerable<Hero>? allHeroes = await _unitOfWorkRepository.HeroRepository.GetAllHeroesAsync();

            List<Hero> allHeroesByPageSizeAndNumber = allHeroes.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                                             .Take(paginationFilter.PageSize)
                                                             .ToList();
            return allHeroesByPageSizeAndNumber;
        }

        private async Task<List<Hero>> HeroesWithSorting(string? searchString, string sortBy, PaginationFilter validFilter)
        {
            List<Hero> allHeroesByPageSizeAndNumber = await GetHeroesPagination(validFilter);

            List<Hero> allHeroesSortBy = allHeroesByPageSizeAndNumber.OrderByProperty(sortBy).ToList();

            if (searchString is not null)
            {
                allHeroesSortBy = HeroesFiltering(searchString, allHeroesSortBy);
            }
            return allHeroesSortBy;
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

        private async Task<List<Hero>> HeroesWithoutSorting(string? searchString, PaginationFilter validFilter)
        {
            List<Hero> allHeroesByPageSizeAndNumber = await GetHeroesPagination(validFilter);

            allHeroesByPageSizeAndNumber = HeroesFiltering(searchString, allHeroesByPageSizeAndNumber);

            return allHeroesByPageSizeAndNumber;
        }


        #endregion Helper Methods

    }
}
