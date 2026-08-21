using Consolidado.Application.Dtos;
using Consolidado.Application.Ports;

namespace Consolidado.Application;

/// <summary>
/// Consulta do saldo do dia. Cache-aside: tenta o Redis primeiro, cai no banco
/// no miss e repovoa o cache. É o caminho quente — alvo dos 50 req/s de pico.
/// </summary>
public class ConsultarSaldoService
{
    private readonly ISaldoDiarioRepository _saldos;
    private readonly ISaldoCache _cache;

    public ConsultarSaldoService(ISaldoDiarioRepository saldos, ISaldoCache cache)
    {
        _saldos = saldos;
        _cache = cache;
    }

    public async Task<SaldoDiarioDto?> ConsultarAsync(DateOnly data, CancellationToken ct = default)
    {
        var emCache = await _cache.ObterAsync(data, ct);
        if (emCache is not null)
            return emCache;

        var saldo = await _saldos.ObterPorDataAsync(data, ct);
        if (saldo is null)
            return null;

        var dto = new SaldoDiarioDto(
            saldo.Data, saldo.TotalCreditos, saldo.TotalDebitos, saldo.Saldo, saldo.AtualizadoEmUtc);

        await _cache.GravarAsync(dto, ct);
        return dto;
    }
}
