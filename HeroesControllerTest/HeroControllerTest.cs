using HeroesAPI.Controllers;
using HeroesAPI.Entitites.Models;
using HeroesAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HeroesControllerTest
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

        [Fact]
        public async Task GetHeroes_ReturnsSuccess()
        {
            FillHeroes();
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetAllHeroesAsync()).ReturnsAsync(_heroes);
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            // Act
            OkObjectResult? okResult = await heroController.GetAllHeroes(string.Empty, "10", new HeroesAPI.Paging.PaginationFilter()) as OkObjectResult;

            // Assert
            Assert.Equal(okResult.StatusCode, 200);
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
                LastName = "Clark",
                Place = "Village",
            });
        }

        [Fact]
        public async Task GetHeroById_ReturnsSuccess()
        {
            FillHeroes();
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(1)).Returns(Task.FromResult(_heroes.FirstOrDefault()));
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            // Act
            ActionResult<Hero>? getOneHero = await heroController.GetOneHero(1);
            Assert.NotNull(getOneHero);
            Assert.NotNull(getOneHero.Result);

            var okResult = (OkObjectResult)getOneHero.Result;
            var actualHero = okResult.Value as Hero;

            // Assert
            Assert.NotNull(actualHero);
            Assert.Contains(_heroes.FirstOrDefault().Name, actualHero.Name);
            Assert.Equal(okResult.StatusCode, 200);
        }





    }


}