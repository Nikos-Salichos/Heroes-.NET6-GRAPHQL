using HeroesAPI.Models;
using HeroesAPI.Paging;
using HeroesAPI.Sorting;

namespace HeroesAPI.Repository
{
    public class HeroRepository : GenericRepository<Hero>, IHeroRepository
    {
        public HeroRepository(MainDbContextInfo msSql) : base(msSql)
        {
        }

        public async Task<List<Hero>> GetAllHeroesAsync()
        {
            return await FindAll().ToListAsync();
        }

        public async Task<Hero?> GetHeroByIdAsync(int heroId)
        {
            return await FindByCondition(hero => hero.Id.Equals(heroId))
                .FirstOrDefaultAsync();
        }

        public void CreateHero(Hero hero)
        {
            Create(hero);
        }

        public void UpdateHero(Hero hero)
        {
            Update(hero);
        }
        public void DeleteHero(Hero hero)
        {
            Delete(hero);
        }

        public string CreateImageDirectory()
        {
            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Resources", "Images"));
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            return pathToSave;
        }

        public void SaveImageInDir(Hero hero, string pathToSave, out string fullPath, out string extension)
        {
            string imageName = Guid.NewGuid().ToString();
            fullPath = Path.Combine(pathToSave, imageName);
            if (hero.Image is not null)
            {
                extension = Path.GetExtension(hero.Image.FileName);
                using (FileStream fileStream = System.IO.File.Create(fullPath + imageName + extension))
                {
                    hero.Image.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }
            else
            {
                extension = string.Empty;
            }
        }

        public (List<Hero>, PaginationFilter) HeroesWithSorting(IEnumerable<Hero> allHeroes, string? searchString, string sortBy, PaginationFilter paginationFilter)
        {


            List<Hero> heroesPagination = allHeroes.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                                             .Take(paginationFilter.PageSize)
                                                             .ToList();


            heroesPagination = heroesPagination.OrderByProperty(sortBy).ToList();


            if (searchString is not null)
            {
                heroesPagination = heroesPagination.Where(h => h.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                                     .ToList();
            }

            return (heroesPagination, paginationFilter);
        }

        public (List<Hero>, PaginationFilter) HeroesWithoutSorting(IEnumerable<Hero> allHeroes, string? searchString, PaginationFilter paginationFilter)
        {

            List<Hero> heroesPagination = allHeroes.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                                             .Take(paginationFilter.PageSize)
                                                             .ToList();

            if (searchString is not null)
            {
                heroesPagination = heroesPagination.Where(h => h.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                                     .ToList();
            }

            return (heroesPagination, paginationFilter);
        }
    }
}
