using Challenger.EasyFlow.Domain.Aggregates.AuditLogAggregate;
using Challenger.EasyFlow.Infrastructure.Database.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", DatabaseSchemas.Audit);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Entity).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("PerformedAt").IsRequired();
    }
}
