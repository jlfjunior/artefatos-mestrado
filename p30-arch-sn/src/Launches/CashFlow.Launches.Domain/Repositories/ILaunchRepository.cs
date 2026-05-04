using CashFlow.Launches.Domain.Entities;

namespace CashFlow.Launches.Domain.Repositories;

public interface ILaunchRepository
{
    Task AddAsync(Launch launch, CancellationToken cancellationToken = default);
}