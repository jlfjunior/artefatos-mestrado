using BCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BCash.Infrastructure.Data
{
    public class SqlDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Balance> Balances { get; set; }

        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Transaction>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Balance>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Balance>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");
        }

        public override int SaveChanges()
        {
            var modifiedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entity in modifiedEntities)
            {
                entity.Property("UpdatedAt").CurrentValue = DateTime.Now;
            }

            return base.SaveChanges();
        }
    }
}