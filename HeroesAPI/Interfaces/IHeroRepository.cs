using HeroesAPI.Models;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Task<List<Hero>> GetAllHeroesAsync();
        Task<Hero?> GetHeroByIdAsync(int heroId);
        bool CreateHero(Hero hero);
        bool UpdateHero(Hero hero);
        void DeleteHero(Hero hero);
        string CreateImageDirectory();
        void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension);
    }
}
