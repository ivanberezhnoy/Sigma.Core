using Microsoft.EntityFrameworkCore;

namespace Sigma.Core.DatabaseEntity
{
    public class DatabaseContext : DbContext
    {
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<School> Schools { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<City>(e => e.Property(o => o.Id));
        }
    }
}
