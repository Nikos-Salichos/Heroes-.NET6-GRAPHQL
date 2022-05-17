using HeroesAPI.Models;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Task<List<Hero>> GetAllHeroesAsync();
        Task<Hero?> GetHeroByIdAsync(int heroId);

        Hero CreateHero(Hero hero);
        Hero UpdateHero(Hero hero);
        Hero DeleteHero(Hero hero);
        string CreateImageDirectory();
        void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension);
    }
}
