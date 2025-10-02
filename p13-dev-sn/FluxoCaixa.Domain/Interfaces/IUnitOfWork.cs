namespace FluxoCaixa.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
}
