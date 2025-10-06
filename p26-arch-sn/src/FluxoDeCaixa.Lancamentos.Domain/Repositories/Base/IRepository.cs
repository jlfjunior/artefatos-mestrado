using System;

namespace FluxoDeCaixa.Caixa.Domain.Repositories.Base
{
    public interface IRepository<T> : IDisposable where T : IAggregationRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
