using System.Text.Json;
using FluxoCaixa.Consolidado.Domain.Repositories;
using StackExchange.Redis;

namespace FluxoCaixa.Consolidado.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task RemoveAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        var db = _redis.GetDatabase();
        var serialized = JsonSerializer.Serialize(value);

        await db.StringSetAsync(key, serialized, expirationTime ?? TimeSpan.FromMinutes(1)); // Default TTL=60s
    }
}
