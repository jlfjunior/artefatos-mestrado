using FinancialBalance.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FinancialBalance.Infrastructure.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
        => _context = context;

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Account?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default)
        => await _context.Accounts
            .Include(a => a.Transactions.Where(t => t.Status != TransactionStatus.Cancelled))
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Account?> GetByIdWithTransactionAsync(Guid accountId, Guid transactionId, CancellationToken ct = default)
        => await _context.Accounts
            .Include(a => a.Transactions.Where(t => t.Id == transactionId))
            .FirstOrDefaultAsync(a => a.Id == accountId, ct);

    public async Task<Account?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _context.Accounts
            .FirstOrDefaultAsync(a => a.Code == code.ToUpperInvariant(), ct);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        => await _context.Accounts
            .AnyAsync(a => a.Code == code.ToUpperInvariant(), ct);

    public async Task AddAsync(Account entity, CancellationToken ct = default)
        => await _context.Accounts.AddAsync(entity, ct);

    public void Update(Account entity)
        => _context.Entry(entity).State = EntityState.Modified;

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task<(IReadOnlyList<Account> Items, int TotalCount)> ListAsync(
        bool? isActive, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Accounts.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> ListTransactionsAsync(
        Guid accountId,
        TransactionType? type,
        TransactionCategory? category,
        TransactionStatus? status,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId);

        if (type.HasValue)     query = query.Where(t => t.Type == type.Value);
        if (category.HasValue) query = query.Where(t => t.Category == category.Value);
        if (status.HasValue)   query = query.Where(t => t.Status == status.Value);
        if (from.HasValue)     query = query.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue)       query = query.Where(t => t.TransactionDate <= to.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
