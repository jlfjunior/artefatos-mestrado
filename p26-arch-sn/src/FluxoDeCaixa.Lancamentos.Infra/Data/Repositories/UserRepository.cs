using FluxoDeCaixa.Caixa.Domain.Entities;
using FluxoDeCaixa.Caixa.Domain.Repositories;
using FluxoDeCaixa.Caixa.Domain.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Caixa.Infra.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CashierContext _context;

        public UserRepository(CashierContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<User> GetUserByCpf(string cpf)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Cpf == cpf);
        }

        public async Task<User> GetUserById(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public void Insert(User user)
        {
            _context.Users.Add(user);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }        
    }
}
