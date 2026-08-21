using Consolidado.Application.Dtos;
using Consolidado.Application.Ports;
using Consolidado.Domain;
using Microsoft.Extensions.Logging;

namespace Consolidado.Application;

/// <summary>
/// Caso de uso disparado pela chegada de um lançamento. Atualiza a projeção do
/// saldo do dia de forma idempotente (chave: LancamentoId) e invalida o cache.
/// </summary>
public class AtualizarSaldoService
{
    private readonly ISaldoDiarioRepository _saldos;
    private readonly ILancamentosProcessadosStore _processados;
    private readonly ISaldoCache _cache;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AtualizarSaldoService> _log;

    public AtualizarSaldoService(
        ISaldoDiarioRepository saldos,
        ILancamentosProcessadosStore processados,
        ISaldoCache cache,
        IUnitOfWork uow,
        ILogger<AtualizarSaldoService> log)
    {
        _saldos = saldos;
        _processados = processados;
        _cache = cache;
        _uow = uow;
        _log = log;
    }

    public async Task ExecutarAsync(AtualizarSaldoCommand cmd, CancellationToken ct = default)
    {
        if (await _processados.JaProcessadoAsync(cmd.LancamentoId, ct))
        {
            // Reentrega. Não toca no saldo de novo.
            _log.LogInformation("Lançamento {LancamentoId} já processado; ignorando reentrega.", cmd.LancamentoId);
            return;
        }

        var saldo = await _saldos.ObterPorDataAsync(cmd.Data, ct) ?? new SaldoDiario(cmd.Data);

        if (string.Equals(cmd.Tipo, "Credito", StringComparison.OrdinalIgnoreCase))
            saldo.AplicarCredito(cmd.Valor);
        else if (string.Equals(cmd.Tipo, "Debito", StringComparison.OrdinalIgnoreCase))
            saldo.AplicarDebito(cmd.Valor);
        else
            throw new InvalidOperationException($"Tipo de lançamento desconhecido: {cmd.Tipo}");

        await _saldos.SalvarAsync(saldo, ct);
        await _processados.MarcarComoProcessadoAsync(cmd.LancamentoId, ct);

        // Projeção e idempotência confirmadas juntas.
        await _uow.CommitAsync(ct);

        // Cache-aside: invalida para a próxima leitura repovoar com o valor novo.
        await _cache.InvalidarAsync(cmd.Data, ct);

        _log.LogInformation(
            "Saldo do dia {Data} atualizado a partir do lançamento {LancamentoId}. Saldo atual: {Saldo}.",
            cmd.Data, cmd.LancamentoId, saldo.Saldo);
    }
}
