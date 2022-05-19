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

        public async Task<bool> Create(T entity)
        {
            MsSql.Set<T>().Add(entity);
            int created = await MsSql.SaveChangesAsync();
            return created > 0;
        }

        public async Task<bool> Update(T entity)
        {
            MsSql.Set<T>().Update(entity);
            int updated = await MsSql.SaveChangesAsync();
            return updated > 0;
        }
        public async Task Delete(T entity)
        {
            MsSql.Set<T>().Remove(entity);
            await MsSql.SaveChangesAsync();
        }
    }
}
