using Flow.Core.DTOs;
using Flow.Core.Entities;
using Flow.Core.Repositories;
using Flow.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Flow.Infra.Repositories
{
    public class UserReadOnlyRepository : IUserReadOnlyRepository
    {
        private readonly RelationalDbContext _context;
        public UserReadOnlyRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByLogin(string login) => 
            await _context.Users.FirstOrDefaultAsync(u => u.Login.Equals(login));

        public async Task<bool> IsUserExistsById(Guid userId) =>
            await _context.Users.AnyAsync(u => u.Id.Equals(userId));
    }
}
