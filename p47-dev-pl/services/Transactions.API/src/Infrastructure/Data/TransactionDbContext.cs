using Microsoft.EntityFrameworkCore;
using Transactions.API.Domain.Models;

namespace Transactions.API.Infrastructure.Data;

public class TransactionDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Lojista)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnType("varchar(2)")
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.TransactionDate)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValue(DateTime.UtcNow);

            entity.Property(e => e.IsProcessed)
                .HasDefaultValue(false);

            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
