using System.Text.Json;
using Consolidado.Application.Dtos;
using Consolidado.Application.Ports;
using Microsoft.Extensions.Caching.Distributed;

namespace Consolidado.Infrastructure.Caching;

/// <summary>
/// Cache-aside sobre Redis (IDistributedCache). Chave por data. TTL curto
/// porque o consumer invalida na escrita — o TTL é só uma rede de segurança
/// contra entradas órfãs.
/// </summary>
public class RedisSaldoCache : ISaldoCache
{
    private static readonly DistributedCacheEntryOptions Opcoes = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private readonly IDistributedCache _cache;

    public RedisSaldoCache(IDistributedCache cache) => _cache = cache;

    private static string Chave(DateOnly data) => $"saldo:{data:yyyy-MM-dd}";

    public async Task<SaldoDiarioDto?> ObterAsync(DateOnly data, CancellationToken ct = default)
    {
        var json = await _cache.GetStringAsync(Chave(data), ct);
        return json is null ? null : JsonSerializer.Deserialize<SaldoDiarioDto>(json);
    }

    public Task GravarAsync(SaldoDiarioDto saldo, CancellationToken ct = default)
        => _cache.SetStringAsync(Chave(saldo.Data), JsonSerializer.Serialize(saldo), Opcoes, ct);

    public Task InvalidarAsync(DateOnly data, CancellationToken ct = default)
        => _cache.RemoveAsync(Chave(data), ct);
}
