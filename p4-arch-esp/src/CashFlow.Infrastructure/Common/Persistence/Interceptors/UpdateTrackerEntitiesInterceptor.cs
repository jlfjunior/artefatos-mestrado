using CashFlow.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CashFlow.Infrastructure.Common.Persistence.Interceptors
{
    public sealed class UpdateTrackerEntitiesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void UpdateEntities(DbContextEventData eventData)
        {
            var dbContext = eventData.Context ?? throw new NullReferenceException();

            var entities =
                dbContext.ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.State == EntityState.Modified).ToList();

            foreach (var entityEntry in entities)
                entityEntry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
