using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Infra.Data
{
    public class CashierContext : DbContext, IUnitOfWork
    {
        public CashierContext(DbContextOptions<CashierContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Entry> Entries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasData(
                new Account()
                {
                    Id = Guid.NewGuid(),
                    Value = 0.0,
                    LastUpdate = DateTime.UtcNow,
                });
        }

        public async Task<bool> Commit()
        {
            var success = await base.SaveChangesAsync() > 0;

            return success;
        }
    }
}
