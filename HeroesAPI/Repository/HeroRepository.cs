using HeroesAPI.Models;

namespace HeroesAPI.Repository
{
    public class HeroRepository : GenericRepository<Hero>, IHeroRepository
    {
        public HeroRepository(MsSql msSql) : base(msSql)
        {
        }

        public async Task<List<Hero>> GetAllHeroesAsync()
        {
            return await FindAll().ToListAsync();
        }

        public async Task<Hero> GetHeroByIdAsync(int heroId)
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

    }
}
