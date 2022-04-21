using HeroesAPI.Controllers;
using HeroesAPI.Entitites.Models;
using HeroesAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HeroesControllerTest
{
    public class HeroControllerTest
    {
        private readonly HeroController _heroController;
        private readonly ILogger<HeroController> _logger;
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        private readonly List<Hero> _heroes = new List<Hero>();

        private readonly Mock<IHeroRepository> _mockHeroRepository;

        private readonly Mock<IUnitOfWorkRepository> _mockUnitOfWorkRepository;

        public HeroControllerTest()
        {
            _mockHeroRepository = new Mock<IHeroRepository>();
            // _heroController = new HeroController(_logger, _unitOfWorkRepository);

            _mockUnitOfWorkRepository = new Mock<IUnitOfWorkRepository> { CallBase = true };
        }

        [Fact]
        public async Task GetHeroById_ReturnsSuccess()
        {
            Hero? newHero = new Hero();
            newHero.Id = 1;
            newHero.Name = "Ironman";
            newHero.FirstName = "Tony";
            newHero.LastName = "Stark";
            newHero.Place = "Long island";

            _mockHeroRepository.Setup(repo => repo.GetHeroByIdAsync(1)).Returns(Task.FromResult(newHero));
            _mockUnitOfWorkRepository.Setup(repo => repo.HeroRepository.GetHeroByIdAsync(1)).Returns(Task.FromResult(newHero));
            HeroController? heroController = new HeroController(_logger, _mockUnitOfWorkRepository.Object);

            // Act
            ActionResult<Hero>? getOneHero = await heroController.GetOneHero(1);

            // Assert
            Assert.Contains(newHero.Name, "Ironman");
        }
    }


}