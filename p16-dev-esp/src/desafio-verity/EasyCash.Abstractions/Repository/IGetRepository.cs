using EasyCash.Abstractions;

namespace EasyCash.Abstractions.Repository;

public interface IGetRepository<T, TIdType> where T : EntityBase<TIdType>
{
    Task<T?> GetByIdAsync(TIdType id, CancellationToken cancellationToken);
}

public interface IGetAllRepository<T, TIdType> where T : EntityBase<TIdType>
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
}

public interface IAddRepository<in T, TIdType> where T : EntityBase<TIdType>
{
    Task AddAsync(T entity, CancellationToken cancellationToken);
}

public interface IUpdateRepository<in T, TIdType> where T : EntityBase<TIdType>
{
    void Update(T entity);
}

public interface IDeleteRepository<in T, TIdType> where T : EntityBase<TIdType>
{
    void Delete(T entity);
}

public interface IRepository<T, TIdType> :
    IGetRepository<T, TIdType>,
    IGetAllRepository<T, TIdType>,
    IAddRepository<T, TIdType>,
    IUpdateRepository<T, TIdType>,
    IDeleteRepository<T, TIdType>
    where T : EntityBase<TIdType>
{
}