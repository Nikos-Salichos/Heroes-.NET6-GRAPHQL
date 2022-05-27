using HeroesAPI.Models;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Task<(bool IsSuccess, List<Hero>? Heroes, string? ErrorMessage)> GetAllHeroesAsyncMsql();
        Task<Hero?> GetHeroByIdAsyncMsql(int heroId);
        Hero CreateHeroMsql(Hero hero);
        Hero UpdateHeroMsql(Hero hero);
        Task DeleteHeroMsql(Hero hero);

        Task<(bool IsSuccess, List<Hero>? Heroes, string? ErrorMessage)> GetAllHeroesAsyncSqlite();
        Hero CreateHeroSqlite(Hero hero);

        string CreateImageDirectory();
        void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension);
    }
}
