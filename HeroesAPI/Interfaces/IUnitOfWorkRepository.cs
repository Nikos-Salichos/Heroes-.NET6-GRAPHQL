namespace HeroesAPI.Interfaces
{
    public interface IUnitOfWorkRepository : IDisposable
    {
        IHeroRepository HeroRepository { get; }
        int Complete();
    }
}
