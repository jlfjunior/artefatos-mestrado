using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application;

public interface IApplicationDbContext
{
    DbSet<TransactionEntity> Transactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}