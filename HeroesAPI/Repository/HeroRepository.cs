using HeroesAPI.Entities.Models;
using HeroesAPI.Interfaces;
using HeroesAPI.Repository.GenericRepository;

namespace HeroesAPI.Repository
{
    public class HeroRepository : GenericRepository<Hero>, IHeroRepository
    {
        public HeroRepository(DataContext context) : base(context)
        {
        }

        public Hero GetByHeroId(int id)
        {
            return _dataContext.Heroes.Where(h => h.Id == id).FirstOrDefault();
        }
    }
}
