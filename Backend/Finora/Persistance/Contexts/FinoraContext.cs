using Finora.Models;
using Microsoft.EntityFrameworkCore;

namespace Finora.Persistance.Contexts
{
    internal class FinoraDbContext : DbContext
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
        }
    }
}
