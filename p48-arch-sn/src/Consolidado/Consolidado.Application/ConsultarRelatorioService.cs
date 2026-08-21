using Consolidado.Application.Dtos;
using Consolidado.Application.Ports;

namespace Consolidado.Application;

/// <summary>
/// Relatório do saldo diário consolidado num período. Diferente da consulta de um
/// dia (caminho quente, cacheável), o relatório varre o intervalo no read model e
/// agrega os totais. Lista apenas os dias que tiveram movimento.
/// </summary>
public class ConsultarRelatorioService
{
    private readonly ISaldoDiarioRepository _saldos;

    public ConsultarRelatorioService(ISaldoDiarioRepository saldos) => _saldos = saldos;

    public async Task<RelatorioConsolidadoDto> GerarAsync(DateOnly de, DateOnly ate, CancellationToken ct = default)
    {
        if (ate < de)
            throw new ArgumentException("A data final não pode ser anterior à inicial.", nameof(ate));

        var saldos = await _saldos.ListarPorPeriodoAsync(de, ate, ct);

        var dias = saldos
            .Select(s => new SaldoDiarioDto(s.Data, s.TotalCreditos, s.TotalDebitos, s.Saldo, s.AtualizadoEmUtc))
            .ToList();

        return new RelatorioConsolidadoDto(
            de,
            ate,
            dias,
            dias.Sum(d => d.TotalCreditos),
            dias.Sum(d => d.TotalDebitos),
            dias.Sum(d => d.Saldo));
    }
}
