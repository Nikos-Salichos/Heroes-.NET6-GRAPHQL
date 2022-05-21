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
            IEnumerable<SeriLogModel>? logs = await FindAll();
            return logs;
        }
    }
}
