using CashFlow.DailyConsolidated.Domain.Entities;
using CashFlow.DailyConsolidated.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CashFlow.DailyConsolidated.Infrastructure.Repositories
{
    public class CacheRepository : ICacheRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<CacheRepository> _logger;

        public CacheRepository(IConnectionMultiplexer redis, ILogger<CacheRepository> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public Task AddDailyConsolidation(DailyConsolidatedEntity dailyConsolidated)
        {
            var key = dailyConsolidated.Date.ToString("dd_MM_yyyy") + "_dailyConsolidated";
            var db = _redis.GetDatabase();

            var serializedValue = JsonSerializer.Serialize(dailyConsolidated);
            return db.StringSetAsync(key, serializedValue);
        }

        public async Task<DailyConsolidatedEntity?> GetDailyConsolidatedByDate(DateOnly date)
        {
            _logger.LogInformation($"Fetching daily consolidated data for date: {date}");
            var key = date.ToString("dd_MM_yyyy") + "_dailyConsolidated";
            var db = _redis.GetDatabase();

            var cachedValue = await db.StringGetAsync(key);
            if (!cachedValue.HasValue)
                return null;

            return JsonSerializer.Deserialize<DailyConsolidatedEntity>(cachedValue!);
        }
    }
}
