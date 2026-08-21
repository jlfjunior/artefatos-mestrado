using ControleLancamentos.Application.Lancamentos.Dtos;

namespace ControleLancamentos.Application.Lancamentos.Services;

public interface ICriarLancamentoService
{
    Task<CriarLancamentoResponse> ExecutarAsync(
        CriarLancamentoRequest request,
        CancellationToken cancellationToken = default);
}
