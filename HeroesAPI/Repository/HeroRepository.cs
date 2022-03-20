using HeroesAPI.Entities.Models;
using HeroesAPI.Interfaces;
using HeroesAPI.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace HeroesAPI.Repository
{
    public class HeroRepository : GenericRepository<Hero>, IHeroRepository
    {
        public HeroRepository(MsSql msSql, SqLite sqLite)
               : base(msSql, sqLite)
        {
        }

        public async Task<IEnumerable<Hero>> GetAllHeroesAsync()
        {
            return await FindAll().ToListAsync();
        }
        public async Task<Hero> GetHeroByIdAsync(int heroId)
        {
            return await FindByCondition(hero => hero.Id.Equals(heroId))
                .FirstOrDefaultAsync();
        }
        public async Task<Hero> GetHeroWithDetailsAsync(Guid heroId)
        {
            return await FindByCondition(owner => owner.Id.Equals(heroId))
                .Include(ac => ac.Name)
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
