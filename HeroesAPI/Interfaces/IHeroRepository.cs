using HeroesAPI.Entities.Models;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Task<IEnumerable<Hero>> GetAllHeroesAsync();
        Task<Hero> GetHeroByIdAsync(Guid hero);
        void CreateHero(Hero hero);
        void UpdateHero(Hero hero);
        void DeleteHero(Hero hero);
    }
}
