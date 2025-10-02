using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class CarrefourContext : DbContext
    {
        public DbSet<CashEntry> CashEntry { get; set; }
        public CarrefourContext(DbContextOptions<CarrefourContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarrefourContext).Assembly);
        }
    }
}
