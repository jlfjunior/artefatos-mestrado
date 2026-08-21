using Consolidation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consolidation.Infrastructure.Persistence.Configurations
{
    public sealed class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
    {
        public void Configure(EntityTypeBuilder<DailyBalance> builder)
        {
            builder.ToTable("daily_balances");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("id");

            builder.Property(d => d.Date)
                .HasColumnName("date")
                .IsRequired();

            builder.Property(d => d.TotalCredits)
                .HasColumnName("total_credits")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(d => d.TotalDebits)
                .HasColumnName("total_debits")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(d => d.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(d => d.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.Ignore(d => d.Balance);

            builder.HasIndex(d => d.Date)
                .IsUnique();
        }
    }
}
