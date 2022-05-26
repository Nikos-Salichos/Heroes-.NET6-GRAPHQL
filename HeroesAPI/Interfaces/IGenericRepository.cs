using System.Linq.Expressions;

namespace HeroesAPI.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> FindAllMsql();
        Task<IEnumerable<T>> FindByIdMsql(Expression<Func<T, bool>> expression);
        Task Create(T entity);
        Task Update(T entity);
        Task Delete(T entity);
    }
}
