namespace HeroesAPI.DataContext
{
    public class SqliteDataContext : DbContext
    {
        public SqliteDataContext(DbContextOptions<MainDbContextInfo> options) : base(options)
        {
        }


    }
}
