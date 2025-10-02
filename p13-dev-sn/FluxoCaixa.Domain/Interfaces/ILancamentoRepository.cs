using FluxoCaixa.Domain.Entities;


namespace FluxoCaixa.Domain.Interfaces
{
    public interface ILancamentoRepository
    {
        Task AdicionarAsync(Lancamento lancamento);
        Task<Lancamento?> ObterPorIdAsync(Guid id);
        Task<List<Lancamento>> ObterTodosAsync();
        Task AtualizarAsync(Lancamento lancamento);
        Task ExcluirAsync(Lancamento lancamento);
    }
}
