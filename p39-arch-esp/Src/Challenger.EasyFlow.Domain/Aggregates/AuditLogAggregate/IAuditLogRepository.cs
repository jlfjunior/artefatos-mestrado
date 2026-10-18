namespace Challenger.EasyFlow.Domain.Aggregates.AuditLogAggregate;

public interface IAuditLogRepository
{
    ValueTask CreateAsync(AuditLog auditLog);
}