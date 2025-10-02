using EasyCash.Abstractions.Idempotency.Entity;
using EasyCash.Command.Store.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyCash.Command.Store.EntityConfigurations.Idempotency;

internal sealed class IdempotencyConfiguration : IEntityTypeConfiguration<IdempotencyEntity>
{
    public void Configure(EntityTypeBuilder<IdempotencyEntity> builder)
    {
        builder.ToTable(TableNames.Idempotency);

        builder.HasKey(outboxMessage => outboxMessage.Id);

        builder.Property(outboxMessage => outboxMessage.Id)
            .ValueGeneratedNever();

        builder.Property(outboxMessage => outboxMessage.Request)
            .HasComment("Request of the message")
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(outboxMessage => outboxMessage.Response)
            .HasComment("Response of the message")
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.HasIndex(outboxMessage => outboxMessage.Id)
            .IsUnique();
    }
}

