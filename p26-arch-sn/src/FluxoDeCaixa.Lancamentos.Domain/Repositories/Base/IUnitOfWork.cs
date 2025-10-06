using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Domain.Repositories.Base
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();
    }
}
