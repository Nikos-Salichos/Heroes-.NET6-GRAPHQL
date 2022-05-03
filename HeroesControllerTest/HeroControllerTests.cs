using AutoMapper;
using HeroesAPI.Controllers;
using HeroesAPI.DTOs;
using HeroesAPI.Interfaces;
using HeroesAPI.Models;
using HeroesAPI.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HeroTests
{
    public class HeroControllerTests
    {
        private readonly ILogger<HeroController> _logger;

        private readonly List<Hero> _heroes = new List<Hero>();

        private readonly Mock<IHeroRepository> _mockHeroRepository;

        private readonly Mock<IUnitOfWorkRepository> _mockUnitOfWorkRepository;

        private readonly Mock<IMapper> _mapper;

        public HeroControllerTests()
        {
            _mockHeroRepository = new Mock<IHeroRepository>();
            _mockUnitOfWorkRepository = new Mock<IUnitOfWorkRepository>();
            _mapper = new Mock<IMapper>();
        }

        private void FillHeroes()
        {
            _heroes.Add(new Hero()
            {
                Id = 10,
                Name = "Ironman",
                FirstName = "Tony",
                LastName = "Stark",
                Place = "Long Island",
            });
            _heroes.Add(new Hero()
            {
                Id = 11,
                Name = "Superman",
                FirstName = "Clark",
                LastName = "Kent",
                Place = "Krypton",
            });
            _heroes.Add(new Hero()
            {
                Id = 13,
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

            IMapper? mapper = new MapperConfiguration(mapperConfiguration =>
            {
                mapperConfiguration.AddProfile<HeroProfile>();
            }).CreateMapper();

            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object, mapper);

            ActionResult<IEnumerable<HeroDTO>>? actionResult = await heroController.GetAllHeroes(string.Empty, null, new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            IEnumerable<HeroDTO>? data = (IEnumerable<HeroDTO>?)(result?.Value);
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.True(data?.ToList().Count > 1, "Expected count is over 2");
        }

        [Fact]
        public async Task GetHeroesNoSearchSorting_ReturnsSuccess()
        {
            FillHeroes();

            IMapper? mapper = new MapperConfiguration(mapperConfiguration =>
            {
                mapperConfiguration.AddProfile<HeroProfile>();
            }).CreateMapper();

            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object, mapper);

            ActionResult<IEnumerable<HeroDTO>>? actionResult = await heroController.GetAllHeroes(string.Empty, "10", new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            IEnumerable<HeroDTO>? data = (IEnumerable<HeroDTO>?)(result?.Value);
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            Assert.True(data?.ToList().Count > 1, "Expected count is over 2");
        }

        [Fact]
        public async Task GetHeroesSearchSorting_ReturnsSuccess()
        {
            FillHeroes();

            IMapper? mapper = new MapperConfiguration(mapperConfiguration =>
            {
                mapperConfiguration.AddProfile<HeroProfile>();
            }).CreateMapper();

            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object, mapper);

            var actionResult = await heroController.GetAllHeroes("thor", "10", new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            IEnumerable<HeroDTO>? data = (IEnumerable<HeroDTO>?)(result?.Value);
            Assert.NotNull(data);
            Assert.NotEmpty(data);
        }

        [Fact]
        public async Task GetHeroesSearchNoSorting_ReturnsSuccess()
        {
            FillHeroes();

            IMapper? mapper = new MapperConfiguration(mapperConfiguration =>
            {
                mapperConfiguration.AddProfile<HeroProfile>();
            }).CreateMapper();

            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object, mapper);

            var actionResult = await heroController.GetAllHeroes("thor", null, new HeroesAPI.Paging.PaginationFilter());
            Assert.NotNull(actionResult);

            OkObjectResult? result = actionResult?.Result as OkObjectResult;
            Assert.Equal(200, result?.StatusCode);

            IEnumerable<HeroDTO>? data = (IEnumerable<HeroDTO>?)(result?.Value);
            Assert.NotNull(data);
            Assert.NotEmpty(data);
        }

        [Fact]
        public async Task GetHeroById_ReturnsSuccess()
        {
            FillHeroes();

            IMapper? mapper = new MapperConfiguration(mapperConfiguration =>
            {
                mapperConfiguration.AddProfile<HeroProfile>();
            }).CreateMapper();

            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(10)).ReturnsAsync(_heroes.FirstOrDefault());
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object, mapper);

            ActionResult<Hero>? actionResult = await heroController.GetOneHero(10);
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
            IMapper? mapper = new MapperConfiguration(mapperConfiguration =>
            {
                mapperConfiguration.AddProfile<HeroProfile>();
            }).CreateMapper();

            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(10)).ReturnsAsync(_heroes.FirstOrDefault());
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object, mapper);

            ActionResult<Hero>? actionResult = await heroController.GetOneHero(15);

            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Result);

            object? statusCode = actionResult?.Result?.GetType()?.GetProperty("StatusCode")?.GetValue(actionResult.Result, null);

            Assert.Equal(404, (int?)statusCode);
        }

        [Fact]
        public async Task DeleteHero_ReturnsSuccess()
        {
            // Arrange
            FillHeroes();

            IMapper? mapper = new MapperConfiguration(mapperConfiguration =>
            {
                mapperConfiguration.AddProfile<HeroProfile>();
            }).CreateMapper();

            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object, mapper);


            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(10)).ReturnsAsync(new Hero() { });
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.DeleteHero(It.IsAny<Hero>())).Verifiable();

            // Act
            IActionResult? actionResulta = await heroController.DeleteHero(10);

            // Assert
            _mockUnitOfWorkRepository.Verify(repo => repo.HeroRepository.DeleteHero(It.IsAny<Hero>()), Times.Once);

            /*            Assert.NotNull(actionResult);
                        Assert.NotNull(actionResult.Result);

                        OkObjectResult? okResult = actionResult.Result as OkObjectResult;
                        Hero? actualHero = okResult?.Value as Hero;

                        Assert.NotNull(actualHero);
                        Assert.Contains(_heroes.FirstOrDefault()?.Name, actualHero?.Name);
                        Assert.Equal(200, okResult?.StatusCode);*/
        }

    }


}