using EasyCash.Abstractions.Idempotency.Entity;

namespace EasyCash.Abstractions.Idempotency.Interfaces;

public interface IIdempotencyCommandStore
{
    Task<IdempotencyEntity?> GetByIdAsync(string key, CancellationToken cancellationToken);

    Task AddAsync(IdempotencyEntity idempotencyEntity, CancellationToken cancellationToken);

    void MarkRequestAsProcessedAsync(IdempotencyEntity entity);
}