using System.Linq.Expressions;

namespace HeroesAPI.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected MainDbContext MsSql { get; set; }
        protected SqliteContext Sqlite { get; set; }

        public GenericRepository(MainDbContext msSql, SqliteContext sqliteContext)
        {
            MsSql = msSql;
            Sqlite = sqliteContext;
        }

        public async Task<IEnumerable<T>> FindAllMsql()
        {
            return await MsSql.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindByIdMsql(Expression<Func<T, bool>> expression)
        {
            return await MsSql.Set<T>().Where(expression).ToListAsync();
        }

        public async Task CreateMsql(T entity)
        {
            MsSql.Set<T>().Add(entity);
            await MsSql.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            MsSql.Set<T>().Update(entity);
            await MsSql.SaveChangesAsync();
        }
        public async Task Delete(T entity)
        {
            MsSql.Set<T>().Remove(entity);
            await MsSql.SaveChangesAsync();
        }

    }
}
