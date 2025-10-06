using Flow.Core.DTOs;
using Flow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Core.Repositories
{
    public interface IUserReadOnlyRepository
    {
        Task<bool> IsUserExistsById(Guid userId);
        Task<User> GetUserByLogin(string login);
    }
}
