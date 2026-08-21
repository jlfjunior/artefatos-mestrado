using Challenger.EasyFlow.Domain.Aggregates.CashBoxAggregate;
using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;
using Challenger.EasyFlow.Infrastructure.Database.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", DatabaseSchemas.Financial);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type)
            .IsRequired();
        builder.Property(x => x.Amount)
            .IsRequired();
        builder.Property(x => x.Category)
            .HasConversion(
                to => to.Id,
                from => TransactionCategory.FromId(from))
            .IsRequired();
        builder.Property(x => x.PaymentMethod)
            .HasConversion(
                to => to.Id,
                from => PaymentMethod.FromId(from))
            .IsRequired();
        builder.Property(x => x.Description)
            .HasMaxLength(500);
        builder.HasOne<CashBox>()
            .WithMany()
            .HasForeignKey(x => x.CashBoxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}