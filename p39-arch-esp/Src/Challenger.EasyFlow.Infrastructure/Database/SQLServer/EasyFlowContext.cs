using Challenger.EasyFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer;

internal sealed class EasyFlowContext(DbContextOptions<EasyFlowContext> options) : DbContext(options), IUnitOfWork
{
    public async ValueTask CommitAsync(CancellationToken cancellationToken = default)
        => await SaveChangesAsync(cancellationToken);

    public async ValueTask UseTransactionAsync(Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await action();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().AreUnicode(false);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DatabaseSchemas.EasyFlow);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
