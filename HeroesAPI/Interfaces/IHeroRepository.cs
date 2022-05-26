using HeroesAPI.Models;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Task<(bool IsSuccess, List<Hero>? Heroes, string? ErrorMessage)> GetAllHeroesAsync();
        Task<Hero?> GetHeroByIdAsyncMsql(int heroId);
        Hero CreateHero(Hero hero);
        Hero UpdateHero(Hero hero);
        Task DeleteHero(Hero hero);
        string CreateImageDirectory();
        void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension);
    }
}
