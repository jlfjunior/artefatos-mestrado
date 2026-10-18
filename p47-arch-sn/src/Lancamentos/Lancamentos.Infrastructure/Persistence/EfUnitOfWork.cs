using Lancamentos.Application.Ports;

namespace Lancamentos.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly LancamentosDbContext _db;

    public EfUnitOfWork(LancamentosDbContext db) => _db = db;

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
