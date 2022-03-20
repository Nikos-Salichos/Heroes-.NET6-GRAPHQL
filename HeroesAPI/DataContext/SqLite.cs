using HeroesAPI.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace HeroesAPI.DataContext
{
    public class SqLite : DbContext
    {
        public SqLite(DbContextOptions<SqLite> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hero>().HasData(
                 new Hero { Id = 1, Name = "lalalala", FirstName = "Thor", LastName = "Odinson", Place = "Asgard" }
                 );
        }

        public DbSet<Hero> Heroes { get; set; }
    }
}
