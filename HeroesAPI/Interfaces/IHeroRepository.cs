using HeroesAPI.Models;
using HeroesAPI.Paging;

namespace HeroesAPI.Interfaces
{
    public interface IHeroRepository : IGenericRepository<Hero>
    {
        Task<List<Hero>> GetAllHeroesAsync();
        Task<Hero?> GetHeroByIdAsync(int heroId);
        void CreateHero(Hero hero);
        void UpdateHero(Hero hero);
        void DeleteHero(Hero hero);
        string CreateImageDirectory();
        void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension);
        (List<Hero>, PaginationFilter) HeroesWithSorting(IEnumerable<Hero> allHeroes, string? searchString, string sortBy, PaginationFilter paginationFilter);
        (List<Hero>, PaginationFilter) HeroesWithoutSorting(IEnumerable<Hero> allHeroes, string? searchString, PaginationFilter paginationFilter);
    }
}
