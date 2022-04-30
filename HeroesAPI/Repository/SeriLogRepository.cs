using HeroesAPI.Models;

namespace HeroesAPI.Repository
{
    public class SeriLogRepository : GenericRepository<SeriLogModel>, ISeriLogRepository
    {
        public SeriLogRepository(MsSql msSql) : base(msSql)
        {
        }

        public async Task<IEnumerable<SeriLogModel>> GetAllLogsAsync()
        {
            return await FindAll().ToListAsync();
        }
    }
}
