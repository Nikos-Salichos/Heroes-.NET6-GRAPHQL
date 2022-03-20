using Microsoft.EntityFrameworkCore;

namespace HeroesAPI.Data
{
    public class SqLite : DbContext
    {
        public SqLite(DbContextOptions<SqLite> options) : base(options)
        {
        }
    }
}
