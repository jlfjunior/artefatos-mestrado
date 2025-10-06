using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Repositories;
using FluxoDeCaixa.Caixa.Domain.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Infra.Data.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly CashierContext _context;

        public AccountRepository(CashierContext cashierContext)
        {
            _context = cashierContext;
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<Account> GetAccount()
        {
            // Como existe apenas uma conta no exemplo, será feito dessa forma
            return await _context.Accounts.FirstAsync();
        }

        public void Update(Account account)
        {
            _context.Accounts.Update(account);
        }

        public void AddEntry(Entry entry)
        {
            _context.Entries.Add(entry);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }        
    }
}
