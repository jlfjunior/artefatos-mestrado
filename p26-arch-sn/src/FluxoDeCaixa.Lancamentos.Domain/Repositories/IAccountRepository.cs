using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Repositories.Base;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Domain.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<Account> GetAccount();

        void Update(Account account);

        void AddEntry(Entry entry);
    }
}
