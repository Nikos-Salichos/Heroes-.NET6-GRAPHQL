namespace HeroesAPI.Repository
{
    public class UnitOfWorkRepository : IUnitOfWorkRepository
    {
        private readonly MainDbContextInfo _msSql;

        public IHeroRepository HeroRepository { get; }

        public IAuthRepository UserRepository { get; }

        public IEmailSenderRepository EmailSenderRepository { get; }

        public IBarcodeRepository BarcodeRepository { get; }

        public IQRCodeRepository QRCodeRepository { get; }

        public ISeriLogRepository SeriLogRepository { get; }

        public IAuthRepository AuthRepository { get; }

        public UnitOfWorkRepository(MainDbContextInfo msSql,
            IHeroRepository heroRepository,
            IAuthRepository userRepository,
            IEmailSenderRepository emailSenderRepository,
            IBarcodeRepository barcodeRepository,
            IQRCodeRepository qRCodeRepository,
            ISeriLogRepository seriLogRepository,
            IAuthRepository authRepository)
        {
            _msSql = msSql;
            HeroRepository = heroRepository;
            UserRepository = userRepository;
            EmailSenderRepository = emailSenderRepository;
            BarcodeRepository = barcodeRepository;
            QRCodeRepository = qRCodeRepository;
            SeriLogRepository = seriLogRepository;
            AuthRepository = authRepository;
        }

        public async Task CommitAll()
        {
            var transaction = _msSql.Database.BeginTransaction();
            try
            {
                Task<int>? saveToDb = _msSql.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction?.Rollback();
            }
        }

        protected virtual void Dispose(bool disposing)
        {

        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }



    }
}
