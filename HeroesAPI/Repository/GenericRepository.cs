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
        public async void Create(T entity)
        {
            await MsSql.Set<T>().AddAsync(entity);
            await MsSql.SaveChangesAsync();
        }
        public void Update(T entity)
        {
            MsSql.Set<T>().Update(entity);
            MsSql.SaveChanges();
        }
        public void Delete(T entity)
        {
            MsSql.Set<T>().Remove(entity);
            MsSql.SaveChanges();
        }

    }
}
