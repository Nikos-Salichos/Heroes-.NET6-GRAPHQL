using HeroesAPI.Interfaces;

namespace HeroesAPI.Repository.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DataContext _dataContext;
        public GenericRepository(DataContext dbcontext)
        {
            _dataContext = dbcontext;
        }

        public void Add(T entity)
        {
            _dataContext.Set<T>().Add(entity);
        }
        public T GetById(int id)
        {
            return _dataContext.Set<T>().Find(id);
        }
        public void Remove(T entity)
        {
            _dataContext.Set<T>().Remove(entity);
        }
        public IEnumerable<T> GetAll()
        {
            return _dataContext.Set<T>().ToList();
        }
        public int Complete()
        {
            return _dataContext.SaveChanges();
        }
    }
}
