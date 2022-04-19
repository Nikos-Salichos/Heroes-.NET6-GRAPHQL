using HeroesAPI.Controllers;
using HeroesAPI.Interfaces;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HeroesControllerTest
{
    public class HeroControllerTest
    {
        private readonly HeroController _heroController;

        private readonly ILogger<HeroController> _logger;
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;


        public HeroControllerTest()
        {

        }

        [Fact]
        public void Get_ReturnAllHeroes()
        {

        }
    }
}