
using HeroesAPI.Entitites.Models;
using HeroesAPI.Interfaces;
using HeroesAPI.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace HeroesAPI.Repository
{
    public class SeriLogRepository : GenericRepository<SeriLogModel>, ISeriLogRepository
    {
        public SeriLogRepository(MsSql msSql, SqLite sqLite) : base(msSql, sqLite)
        {
        }

        public async Task<IEnumerable<SeriLogModel>> GetAllLogsAsync()
        {
            return await FindAll().ToListAsync();
        }
    }
}
