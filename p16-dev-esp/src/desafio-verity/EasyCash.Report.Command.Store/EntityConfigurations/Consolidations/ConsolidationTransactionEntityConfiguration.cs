using EasyCash.Report.Command.Store.Constants;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyCash.Report.Command.Store.EntityConfigurations.Consolidations;

internal sealed class ConsolidationTransactionEntityConfiguration : IEntityTypeConfiguration<ConsolidationTransactionEntity>
{
    public void Configure(EntityTypeBuilder<ConsolidationTransactionEntity> builder)
    {
        builder.ToTable(TableNames.Transactions);

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .ValueGeneratedNever();

        builder.Property(transaction => transaction.Date)
            .HasComment("Date of the transaction")
            .HasConversion(
                v => v.UtcDateTime,
                v => new DateTimeOffset(v, TimeSpan.Zero)
            )
            .IsRequired();

        builder.Property(transaction => transaction.Description)
            .HasComment("Description of the transaction")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(transaction => transaction.Type)
            .HasComment("Type of the transaction")
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasConversion(x => x.Amount, x => Money.FromDecimal(x))
            .HasComment("Amount of the transaction")
            .IsRequired();

        builder.Property(transaction => transaction.CreatedAt)
            .HasComment("Date when the transaction was created")
            .HasConversion(
                v => v.UtcDateTime,
                v => new DateTimeOffset(v, TimeSpan.Zero)
            )
            .IsRequired();
    }
}

