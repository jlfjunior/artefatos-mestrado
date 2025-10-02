using EasyCash.Command.Store.Constants;
using EasyCash.Domain.Transactions.Entities;
using EasyCash.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyCash.Command.Store.EntityConfigurations.Transactions;

internal sealed class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
{
    public void Configure(EntityTypeBuilder<TransactionEntity> builder)
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

        builder.Property(transaction => transaction.Category)
            .HasComment("Category of the transaction")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(transaction => transaction.CreatedAt)
            .HasComment("Date when the transaction was created")
            .HasConversion(
                v => v.UtcDateTime,
                v => new DateTimeOffset(v, TimeSpan.Zero)
            )
            .IsRequired();

        builder.Property(transaction => transaction.UpdatedAt)
            .HasComment("Date when the transaction was updated")
            .HasConversion(
                v => v.Value.UtcDateTime,
                v => new DateTimeOffset(v, TimeSpan.Zero)
            )
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .HasComment("Concurrency Token")
            .HasDefaultValueSql("1")
            .IsConcurrencyToken()
            .IsRequired();

        builder.HasIndex(transaction => transaction.Date);
    }
}