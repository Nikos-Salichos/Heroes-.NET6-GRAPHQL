namespace HeroesAPI.Interfaces
{
    public interface IUnitOfWorkRepository : IDisposable
    {
        IHeroRepository HeroRepository { get; }
        IAuthRepository UserRepository { get; }
        IEmailSenderRepository EmailSenderRepository { get; }
        int Complete();
    }
}
