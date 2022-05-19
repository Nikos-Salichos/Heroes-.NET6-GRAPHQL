using System.Runtime.CompilerServices;

namespace HeroesAPI.Interfaces
{
    public interface IUnitOfWorkRepository : IDisposable
    {
        IHeroRepository HeroRepository { get; }
        IAuthRepository UserRepository { get; }
        IEmailSenderRepository EmailSenderRepository { get; }
        IBarcodeRepository BarcodeRepository { get; }
        IQRCodeRepository QRCodeRepository { get; }
        ISeriLogRepository SeriLogRepository { get; }
        IAuthRepository AuthRepository { get; }

        public string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
