using EasyCash.Abstractions;
using EasyCash.Command.Store.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EasyCash.Command.Store.Repositories;

internal abstract class Repository<T, TIdType>
    where T : EntityBase<TIdType>
{
    protected readonly ApplicationDbContext DbContext;

    protected Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<T?> GetByIdAsync(
        TIdType id,
        CancellationToken cancellationToken = default)
    {
        if (id is null)
            return default;

        return await DbContext
            .Set<T>()
            .FirstOrDefaultAsync(x => x.Id != null && x.Id.Equals(id), cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbContext.AddAsync(entity, cancellationToken);
    }
}
