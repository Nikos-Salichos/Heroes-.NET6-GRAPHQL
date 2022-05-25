using HeroesAPI.Models;

namespace HeroesAPI.DataContext
{
    public class SqliteContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public SqliteContext(DbContextOptions<SqliteContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // connect to sql server with connection string from app settings
            optionsBuilder.UseSqlite(Configuration.GetConnectionString("SqliteConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hero>().HasData(
                 new Hero { Id = 9, Name = "Thor", FirstName = "Thor", LastName = "Odinson", Place = "Asgard" }
                 );
        }

        public DbSet<Hero> Heroes { get; set; }

    }
}
