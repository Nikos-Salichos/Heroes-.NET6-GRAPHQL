using System.Linq.Expressions;

namespace HeroesAPI.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected MainDbContextInfo MsSql { get; set; }

        public GenericRepository(MainDbContextInfo msSql)
        {
            MsSql = msSql;
        }

        public IQueryable<T> FindAll()
        {
            return MsSql.Set<T>().AsNoTracking();
        }
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return MsSql.Set<T>().Where(expression).AsNoTracking();
        }
        public void Create(T entity)
        {
            MsSql.Set<T>().Add(entity);
        }
        public void Update(T entity)
        {
            MsSql.Set<T>().Update(entity);
        }
        public void Delete(T entity)
        {
            MsSql.Set<T>().Remove(entity);
        }

        public async Task SaveAsync()
        {
            await MsSql.SaveChangesAsync();
        }

    }
}
