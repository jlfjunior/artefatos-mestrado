using Consolidado.Application.Ports;

namespace Consolidado.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly ConsolidadoDbContext _db;

    public EfUnitOfWork(ConsolidadoDbContext db) => _db = db;

    public Task CommitAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
