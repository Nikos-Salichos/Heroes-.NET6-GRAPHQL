using HeroesAPI.Models;

namespace HeroesAPI.Repository
{
    public class HeroRepository : GenericRepository<Hero>, IHeroRepository
    {
        public HeroRepository(MainDbContextInfo msSql) : base(msSql)
        {
        }

        public Task<List<Hero>> GetAllHeroesAsync()
        {
            return FindAll().ToListAsync();
        }

        public Task<Hero?> GetHeroByIdAsync(int heroId)
        {
            return FindByCondition(hero => hero.Id.Equals(heroId))
                .FirstOrDefaultAsync();
        }

        public Hero CreateHero(Hero hero)
        {
            Create(hero);
            return hero;
        }

        public Hero UpdateHero(Hero hero)
        {
            Update(hero);
            return hero;
        }

        public async Task DeleteHero(Hero hero)
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

        public void SaveImageInDir(Hero newHero, string pathToSave, out string fullPath, out string extension)
        {
            string imageName = Guid.NewGuid().ToString();
            fullPath = Path.Combine(pathToSave, imageName);
            if (newHero.Image is not null)
            {
                extension = Path.GetExtension(newHero.Image.FileName);
                using (FileStream fileStream = System.IO.File.Create(fullPath + imageName + extension))
                {
                    newHero.Image.CopyTo(fileStream);
                    fileStream.Flush();
                }
            }
            else
            {
                extension = string.Empty;
            }
        }
    }
}
