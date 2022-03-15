using HeroesAPI.Entities.Models;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Hero GetByHeroId(int id);
    }
}
