using Flow.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flow.Infra.Data
{
    public class RelationalDbContext : DbContext
    {
        public RelationalDbContext(DbContextOptions<RelationalDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
            modelBuilder.Entity<User>().HasKey(u => u.Id);
        }
    }
}
