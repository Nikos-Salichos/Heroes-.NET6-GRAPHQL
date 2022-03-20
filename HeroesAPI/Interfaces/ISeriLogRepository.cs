

using HeroesAPI.Entitites.Models;

namespace HeroesAPI.Interfaces
{
    public interface ISeriLogRepository : IGenericRepository<SeriLogModel>
    {
        Task<IEnumerable<SeriLogModel>> GetAllLogsAsync();
    }
}
