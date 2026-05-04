using CashFlow.Launches.Domain.Entities;
using CashFlow.Launches.Domain.Repositories;
using CashFlow.Launches.Infrastructure.Persistence;

namespace CashFlow.Launches.Infrastructure.Repositories;

public sealed class LaunchRepository(LaunchesDbContext dbContext) : ILaunchRepository
{
    public async Task AddAsync(Launch launch, CancellationToken cancellationToken = default)
    {
        await dbContext.Launches.AddAsync(launch, cancellationToken);
    }
}