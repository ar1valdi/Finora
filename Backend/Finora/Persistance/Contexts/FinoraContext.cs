using Finora.Models;
using Microsoft.EntityFrameworkCore;

namespace Finora.Persistance.Contexts
{
    public class FinoraDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public FinoraDbContext(DbContextOptions<FinoraDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Finora");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinoraDbContext).Assembly);

            // Configure DateTime properties to use timestamp without time zone
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }
        }
    }
}
