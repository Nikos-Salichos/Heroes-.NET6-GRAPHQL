using HeroesAPI.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace HeroesAPI.Data
{
    public class MsSql : DbContext
    {
        public MsSql(DbContextOptions<MsSql> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hero>().HasData(
                 new Hero { Id = 9, Name = "Thor", FirstName = "Thor", LastName = "Odinson", Place = "Asgard" }
                 );
        }

        //EF Commands
        // cd C:\Users\Nikos\source\repos\HeroesAPI\HeroesAPI
        // dotnet ef migrations add HeroSeeding
        // dotnet ef database update

        public DbSet<Hero> Heroes { get; set; }
    }
}
