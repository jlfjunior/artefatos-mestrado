using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Domain.Entities;
using CashFlowControl.Core.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CashFlowControl.Core.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Transaction>?> GetAllTransactionsAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

        public async Task<List<Transaction>?> GetTransactionByDateAsync(DateTime date)
        {
            return await _context.Transactions.Where(x => x.CreatedAt.Date == date.Date).ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
        {
            return await _context.Transactions.FindAsync(id);
        }
    }
}
