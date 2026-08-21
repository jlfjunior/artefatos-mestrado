using System.Text.Json;
using FinancialBalance.Application.Common;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;

namespace FinancialBalance.ReportingInfrastructure.Cache;

public class RedisReportCache : IReportCache
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisReportCache> _logger;
    private readonly ResiliencePipeline _pipeline;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisReportCache(IConnectionMultiplexer redis, ILogger<RedisReportCache> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
        _pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),
                OnOpened = args =>
                {
                    logger.LogWarning("Redis circuit breaker opened for {Duration}s", args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation("Redis circuit breaker closed");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        try
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var value = await _db.StringGetAsync(key);
                if (!value.HasValue) return null;
                return JsonSerializer.Deserialize<T>(value!, JsonOptions);
            }, ct);
        }
        catch (BrokenCircuitException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache GET failed for key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class
    {
        try
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                var json = JsonSerializer.Serialize(value, JsonOptions);
                await _db.StringSetAsync(key, json, ttl);
            }, ct);
        }
        catch (BrokenCircuitException) { }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _pipeline.ExecuteAsync(async token =>
                await _db.KeyDeleteAsync(key), ct);
        }
        catch (BrokenCircuitException) { }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE failed for key {Key}", key);
        }
    }
}
