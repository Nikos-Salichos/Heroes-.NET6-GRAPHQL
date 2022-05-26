using System.Linq.Expressions;

namespace HeroesAPI.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> FindAllMsql();
        Task<IEnumerable<T>> FindByIdMsql(Expression<Func<T, bool>> expression);
        Task CreateMsql(T entity);
        Task UpdateMsql(T entity);
        Task DeleteMsql(T entity);
    }
}
