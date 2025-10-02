using System.Linq.Expressions;

namespace Repositories.Repositories.Generic.GenericSQL;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IList<T>> GetAllAsync();
    Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
