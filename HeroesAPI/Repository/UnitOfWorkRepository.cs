namespace HeroesAPI.Repository
{
    public class UnitOfWorkRepository : IUnitOfWorkRepository
    {
        private readonly MsSql _msSql;

        public IHeroRepository HeroRepository { get; }

        public UnitOfWorkRepository(MsSql msSql, IHeroRepository heroRepository)
        {
            _msSql = msSql;
            HeroRepository = heroRepository;
        }

        public int Complete()
        {
            return _msSql.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _msSql.Dispose();
            }
        }
    }
}
