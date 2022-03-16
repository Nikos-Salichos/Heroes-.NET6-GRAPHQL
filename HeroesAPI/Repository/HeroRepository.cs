using HeroesAPI.Entities.Models;
using HeroesAPI.Interfaces;
using HeroesAPI.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace HeroesAPI.Repository
{
    public class HeroRepository : GenericRepository<Hero>, IHeroRepository
    {
        public HeroRepository(DataContext dataContext)
               : base(dataContext)
        {
        }

        public async Task<IEnumerable<Hero>> GetAllHeroesAsync()
        {
            return await FindAll()
               .OrderBy(ow => ow.Name)
               .ToListAsync();
        }
        public async Task<Hero> GetHeroByIdAsync(Guid heroId)
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
