using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boxflux.Domain.Interfaces
{
    public interface IGeralRepository<T>
    {
        Task CreateAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(T item);
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllByDateAsync(DateTime date);
        Task<T> GetByDateAsync(DateTime date);
        Task AddOrUpdateAsync(Guid id, T item);
    }
}
