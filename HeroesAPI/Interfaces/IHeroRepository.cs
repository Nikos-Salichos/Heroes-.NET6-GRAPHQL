using HeroesAPI.Models;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Task<List<Hero>> GetAllHeroesAsync();
        Task<Hero?> GetHeroByIdAsync(int heroId);
        Task<Hero?> CreateHero(Hero hero);
        Task<Hero?> UpdateHero(Hero hero);
        void DeleteHero(Hero hero);
        string CreateImageDirectory();
        void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension);
    }
}
