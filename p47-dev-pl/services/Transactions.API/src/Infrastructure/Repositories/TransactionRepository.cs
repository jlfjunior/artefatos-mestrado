using Microsoft.EntityFrameworkCore;
using Transactions.API.Domain.Interfaces;
using Transactions.API.Domain.Models;
using Transactions.API.Infrastructure.Data;

namespace Transactions.API.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _context;
    private readonly ILogger<TransactionRepository> _logger;

    public TransactionRepository(TransactionDbContext context, ILogger<TransactionRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        _context.Transactions.Add(transaction);
        _logger.LogDebug("Transação adicionada: {TransactionId}", transaction.Id);
        return transaction;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<Transaction>> GetByDateRangeAsync(
        DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;

        var query = _context.Transactions.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query.CountAsync(cancellationToken);
    }
}


