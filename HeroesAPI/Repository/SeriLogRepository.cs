using HeroesAPI.Models;

namespace HeroesAPI.Repository
{
    public class SeriLogRepository : GenericRepository<SeriLogModel>, ISeriLogRepository
    {
        public SeriLogRepository(MainDbContext msSql, SqliteContext sqliteContext) : base(msSql, sqliteContext)
        {
        }

        public async Task<IEnumerable<SeriLogModel>> GetAllLogsAsync()
        {
            IEnumerable<SeriLogModel>? logs = await FindAllMsql();
            return logs;
        }
    }
}
