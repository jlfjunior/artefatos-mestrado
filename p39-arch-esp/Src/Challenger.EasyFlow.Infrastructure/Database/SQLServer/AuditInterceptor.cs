using Challenger.EasyFlow.Application.Common.EventBus;
using Challenger.EasyFlow.Domain.Aggregates.AuditLogAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer;

internal sealed class AuditInterceptor(List<AuditLog> auditLogs, IEventBus eventBus) : SaveChangesInterceptor
{
    private readonly List<AuditLog> _auditLogs = auditLogs;
    private readonly IEventBus _eventBus = eventBus;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var auditLogs = eventData.Context?
            .ChangeTracker
            .Entries()
            .Where(e => e.Entity is not AuditLog
                && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new AuditLog
            {
                Entity = e.DebugView.LongView,
                Action = e.State.ToString(),
                EntityId = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? string.Empty,
                UserId = "System", // Placeholder, replace with actual user context
            });

        if (auditLogs is not null)
            _auditLogs.AddRange(auditLogs);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        // await _eventBus.PublishAsync("audit-logs", _auditLogs, cancellationToken);
        _auditLogs.Clear();

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        // await _eventBus.PublishAsync("audit-logs", _auditLogs, cancellationToken);
        _auditLogs.Clear();

        await base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}