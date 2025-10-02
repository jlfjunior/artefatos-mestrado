using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Domain.Entities;
using CashFlowControl.Core.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CashFlowControl.Core.Infrastructure.Repositories
{
    public class ConsolidatedBalanceRepository : IConsolidatedBalanceRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsolidatedBalanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConsolidatedBalance?> GetBalanceByDateAsync(DateTime date)
        {
            return await _context.ConsolidatedBalances.FirstOrDefaultAsync(b => b.Date == date.Date);
        }

        public async Task CreateBalanceAsync(ConsolidatedBalance balance)
        {
            await _context.ConsolidatedBalances.AddAsync(balance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBalanceAsync(ConsolidatedBalance balance)
        {
            _context.ConsolidatedBalances.Update(balance);
            await _context.SaveChangesAsync();
        }
    }
}
