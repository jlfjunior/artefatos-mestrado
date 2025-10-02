using EasyCash.Abstractions.Repository;
using EasyCash.Domain.Users.Entities;

namespace EasyCash.Domain.Users.Interfaces;

public interface IUserRepository : IGetRepository<UserEntity, Guid>, IAddRepository<UserEntity, Guid>, IUpdateRepository<UserEntity, Guid>
{
}
