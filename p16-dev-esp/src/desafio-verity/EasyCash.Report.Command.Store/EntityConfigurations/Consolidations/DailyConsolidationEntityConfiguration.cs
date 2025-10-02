using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCash.Report.Command.Store.Constants;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyCash.Report.Command.Store.EntityConfigurations.Consolidations;

internal sealed class DailyConsolidationEntityConfiguration : IEntityTypeConfiguration<DailyConsolidationEntity>
{
    public void Configure(EntityTypeBuilder<DailyConsolidationEntity> builder)
    {
        builder.ToTable(TableNames.DailyConsolidations);

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

        builder.Property(transaction => transaction.TotalCredit)
            .HasConversion(x => x.Amount, x => Money.FromDecimal(x))
            .HasComment("Total sum of Credits em the date")
            .IsRequired();

        builder.Property(transaction => transaction.TotalDebit)
            .HasConversion(x => x.Amount, x => Money.FromDecimal(x))
            .HasComment("Total sum of Debits em the date")
            .IsRequired();

        builder.Property(transaction => transaction.FinalBalance)
            .HasConversion(x => x.Amount, x => Money.FromDecimal(x))
            .HasComment("Final balance of the date")
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

