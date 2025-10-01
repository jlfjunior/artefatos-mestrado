using CashFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

namespace CashFlow.Infrastructure.Common.Cache
{
    public abstract class CacheRepositoryBase
    {
        protected const int DefaultCacheDurationInMinutes = 3;
        protected const int DefaultLocalCacheDurationInSeconds = 30;

        protected readonly HybridCache _hybridCache;
        protected readonly ICashFlowDbContext _cashFlowDbContext;

        protected CacheRepositoryBase(ICashFlowDbContext cashFlowDbContext, HybridCache hybridCache)
        {
            _cashFlowDbContext = cashFlowDbContext;
            _hybridCache = hybridCache;
        }

        protected static HybridCacheEntryOptions GetDefaultCacheOptions()
            => new()
            {
                Expiration = TimeSpan.FromMinutes(DefaultCacheDurationInMinutes),
                LocalCacheExpiration = TimeSpan.FromSeconds(DefaultLocalCacheDurationInSeconds)
            };

        protected async Task SetToCacheAsync<T>(string key, T @object, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(@object);
            await _hybridCache.SetAsync(key, json, options: GetDefaultCacheOptions(), tags: null, cancellationToken);
        }
    }
}
