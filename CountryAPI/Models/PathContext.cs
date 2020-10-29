using CountryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class PathContext : DbContext
    {
        public PathContext(DbContextOptions<PathContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CountryConnection>()
                .HasKey(o => new { o.CountryOneID, o.CountryTwoID });
        }


        public DbSet<Country> Countries { get; set; }
        public DbSet<CountryConnection> Connections { get; set; }
    }
}
