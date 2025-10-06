using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Repositories.Base;
using System;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByCpf(string cpf);

        Task<User> GetUserById(Guid id);

        void Insert(User user);
    }
}
