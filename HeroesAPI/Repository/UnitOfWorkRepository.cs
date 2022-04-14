using System.Runtime.CompilerServices;

namespace HeroesAPI.Repository
{
    public class UnitOfWorkRepository : IUnitOfWorkRepository
    {
        private readonly MsSql _msSql;

        public IHeroRepository HeroRepository { get; }

        public IAuthRepository UserRepository { get; }

        public IEmailSenderRepository EmailSenderRepository { get; }

        public IBarcodeRepository BarcodeRepository { get; }

        public UnitOfWorkRepository(MsSql msSql,
            IHeroRepository heroRepository,
            IAuthRepository userRepository,
            IEmailSenderRepository emailSenderRepository,
            IBarcodeRepository barcodeRepository)
        {
            _msSql = msSql;
            HeroRepository = heroRepository;
            UserRepository = userRepository;
            EmailSenderRepository = emailSenderRepository;
            BarcodeRepository = barcodeRepository;
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

        public string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
