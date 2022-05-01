using HeroesAPI.Models;

namespace HeroesAPI.Repository
{
    public class SeriLogRepository : GenericRepository<SeriLogModel>, ISeriLogRepository
    {
        public SeriLogRepository(MainDbContextInfo msSql) : base(msSql)
        {
        }

        public async Task<IEnumerable<SeriLogModel>> GetAllLogsAsync()
        {
            return await FindAll().ToListAsync();
        }
    }
}
