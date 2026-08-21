using Consolidation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Consolidation.Infrastructure.Persistence
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<DailyBalance> DailyBalances => Set<DailyBalance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
