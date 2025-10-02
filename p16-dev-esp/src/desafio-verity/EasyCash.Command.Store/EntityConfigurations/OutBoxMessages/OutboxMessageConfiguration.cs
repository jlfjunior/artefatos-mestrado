using EasyCash.Command.Store.Constants;
using EasyCash.Command.Store.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyCash.Command.Store.EntityConfigurations.OutBoxMessages;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable(TableNames.OutboxMessages);

        builder.HasKey(outboxMessage => outboxMessage.Id);

        builder.Property(outboxMessage => outboxMessage.Id).ValueGeneratedNever();

        builder.Property(outboxMessage => outboxMessage.OccurredOnUtc)
            .IsRequired();

        builder.Property(outboxMessage => outboxMessage.Type)
            .HasComment("Type of class for desserializing")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(outboxMessage => outboxMessage.Content).HasColumnType("jsonb")
            .HasComment("Content of the message")
            .IsRequired();

        builder.Property(outboxMessage => outboxMessage.ProcessedOnUtc)
            .HasComment("Date when the message was processed")
            .IsRequired(false);

        builder.Property(outboxMessage => outboxMessage.Error)
            .HasComment("Error message if the message processing failed")
            .IsRequired(false)
            .HasMaxLength(500);

        builder.HasIndex(outboxMessage => new { outboxMessage.OccurredOnUtc, outboxMessage.ProcessedOnUtc })
            .IncludeProperties(x => new { x.Id, x.Type, x.Content });
    }
}
