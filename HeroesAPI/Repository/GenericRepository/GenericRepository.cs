using HeroesAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HeroesAPI.Repository.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected DataContext DataContext { get; set; }
        public GenericRepository(DataContext dataContext)
        {
            DataContext = dataContext;
        }

        public IQueryable<T> FindAll()
        {
            return DataContext.Set<T>().AsNoTracking();
        }
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return DataContext.Set<T>().Where(expression).AsNoTracking();
        }
        public void Create(T entity)
        {
            DataContext.Set<T>().Add(entity);
        }
        public void Update(T entity)
        {
            DataContext.Set<T>().Update(entity);
        }
        public void Delete(T entity)
        {
            DataContext.Set<T>().Remove(entity);
        }

        public async Task SaveAsync()
        {
            await DataContext.SaveChangesAsync();
        }

    }
}
