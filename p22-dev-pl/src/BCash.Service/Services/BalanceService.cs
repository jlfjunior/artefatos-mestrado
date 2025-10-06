using BCash.Domain.DTOs;
using BCash.Domain.Entities;
using BCash.Domain.Repositories;
using BCash.Domain.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BCash.Service.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly IBalanceRepository _balanceRepository;

        private readonly IDistributedCache _distributedCache;

        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(1);

        public BalanceService(IBalanceRepository balanceService, IDistributedCache distributedCache)
        {
            _balanceRepository = balanceService;
            _distributedCache = distributedCache;
        }

        public async Task<PagedResponseOffset<Balance>> GetBalancePagedAsync(DateTime initDate, DateTime endDate, int pageNumber, int pageSize)
        {
            string cacheKey = $"report-balance-{initDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}-{pageNumber}-{pageSize}";

            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<PagedResponseOffset<Balance>>(cachedData);
            }

            var pagedBalances = await _balanceRepository.GetByDatePagedAsync(initDate, endDate, pageNumber, pageSize);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(pagedBalances),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheExpiry }
                );

            return pagedBalances;
        }

        public async Task<Balance> ProcessBalanceAsync(decimal amount, DateTime date, string type)
        {
            var balance = await _balanceRepository.GetByDateAsync(date);

            decimal totalCredit = type.Equals("C") ? amount : 0;
            decimal totalDebit = type.Equals("D") ? amount : 0;

            if (balance == null)
            {
                balance = new Balance(totalCredit, totalDebit, date);
                return await _balanceRepository.AddAsync(balance);
            }
            else
            {
                balance.TotalCredit = balance.TotalCredit + totalCredit;
                balance.TotalDebit = balance.TotalDebit + totalDebit;
                return await _balanceRepository.UpdateAsync(balance);
            }
        }
    }
}
