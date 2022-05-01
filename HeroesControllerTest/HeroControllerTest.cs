using HeroesAPI.Controllers;
using HeroesAPI.Interfaces;
using HeroesAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HeroController.Test
{
    public class HeroControllerTest
    {
        private readonly ILogger<HeroController> _logger;

        private readonly List<Hero> _heroes = new List<Hero>();

        private readonly Mock<IHeroRepository> _mockHeroRepository;

        private readonly Mock<IUnitOfWorkRepository> _mockUnitOfWorkRepository;

        public HeroControllerTest()
        {
            _mockHeroRepository = new Mock<IHeroRepository>();
            _mockUnitOfWorkRepository = new Mock<IUnitOfWorkRepository>();
        }

        private void FillHeroes()
        {
            _heroes.Add(new Hero()
            {
                Id = 1,
                Name = "Ironman",
                FirstName = "Tony",
                LastName = "Stark",
                Place = "Long Island",
            });
            _heroes.Add(new Hero()
            {
                Id = 2,
                Name = "Superman",
                FirstName = "Clark",
                LastName = "Kent",
                Place = "Krypton",
            });
            _heroes.Add(new Hero()
            {
                Id = 3,
                Name = "Thor",
                FirstName = "Thor",
                LastName = "Odinson",
                Place = "Asgard",
            });
        }

        [Fact]
        public async Task GetHeroesNoSearchNoSorting_ReturnsSuccess()
        {
            FillHeroes();
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            ActionResult<Hero>? actionResult = await heroController.GetAllHeroes(string.Empty, null, new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            object? data = result?.Value?.GetType().GetProperties().First(o => o.Name == "Data").GetValue(result.Value, null);
            List<Hero>? heroesList = data as List<Hero>;
            Assert.NotNull(heroesList);
            Assert.NotEmpty(heroesList);
            Assert.True(heroesList?.Count > 1, "Expected count is over 2");
        }

        [Fact]
        public async Task GetHeroesNoSearchSorting_ReturnsSuccess()
        {
            FillHeroes();
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            ActionResult<Hero>? actionResult = await heroController.GetAllHeroes(string.Empty, "10", new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            object? data = result?.Value?.GetType().GetProperties().First(o => o.Name == "Data").GetValue(result.Value, null);
            List<Hero>? heroesList = data as List<Hero>;
            Assert.NotNull(heroesList);
            Assert.NotEmpty(heroesList);
            Assert.True(heroesList?.Count > 1, "Expected count is over 2");
        }

        [Fact]
        public async Task GetHeroesSearchSorting_ReturnsSuccess()
        {
            FillHeroes();
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            ActionResult<Hero>? actionResult = await heroController.GetAllHeroes("thor", "10", new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            object? data = result?.Value?.GetType().GetProperties().First(o => o.Name == "Data").GetValue(result.Value, null);
            List<Hero>? heroesList = data as List<Hero>;
            Assert.NotNull(heroesList);
            Assert.NotEmpty(heroesList);
        }

        [Fact]
        public async Task GetHeroesSearchNoSorting_ReturnsSuccess()
        {
            FillHeroes();
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            ActionResult<Hero>? actionResult = await heroController.GetAllHeroes("thor", null, new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            object? data = result?.Value?.GetType().GetProperties().First(o => o.Name == "Data").GetValue(result.Value, null);
            List<Hero>? heroesList = data as List<Hero>;
            Assert.NotNull(heroesList);
            Assert.NotEmpty(heroesList);
        }

        [Fact]
        public async Task GetHeroById_ReturnsSuccess()
        {
            FillHeroes();
            //  _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(1)).Returns(Task.FromResult(_heroes.FirstOrDefault()));
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(1)).ReturnsAsync(_heroes[0]);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            ActionResult<Hero>? actionResult = await heroController.GetOneHero(1);
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Result);

            OkObjectResult? okResult = actionResult.Result as OkObjectResult;
            Hero? actualHero = okResult?.Value as Hero;

            Assert.NotNull(actualHero);
            Assert.Contains(_heroes.FirstOrDefault()?.Name, actualHero?.Name);
            Assert.Equal(200, okResult?.StatusCode);
        }


        [Fact]
        public async Task GetHeroById_ReturnsFail()
        {
            FillHeroes();
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(2)).ReturnsAsync(_heroes[0]);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            ActionResult<Hero>? actionResult = await heroController.GetOneHero(1);

            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Result);

            object? statusCode = actionResult?.Result?.GetType()?.GetProperty("StatusCode")?.GetValue(actionResult.Result, null);

            Assert.Equal(404, (int?)statusCode);
        }


    }


}