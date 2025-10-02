using EasyCash.Command.Store.Contexts;
using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EasyCash.Command.Store.Repositories.Users;

internal sealed class UserRepository : Repository<UserEntity, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public void Update(UserEntity entity)
    {
        DbContext.Entry(entity).State = EntityState.Modified;
    }
}
