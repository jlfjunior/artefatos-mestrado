using BCash.Domain.DTOs;
using BCash.Domain.Entities;
using BCash.Domain.Repositories;
using BCash.Domain.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BCash.Service.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        private readonly IDistributedCache _distributedCache;

        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(1);

        public TransactionService(ITransactionRepository transactionRepository, IDistributedCache distributedCache)
        {
            _transactionRepository = transactionRepository;
            _distributedCache = distributedCache;
        }

        public async Task CancelTransactionAsync(Guid id)
        {
            await _transactionRepository.DeleteAsync(id);
        }

        public async Task<Transaction> GetTransactionAsync(Guid id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<PagedResponseOffset<Transaction>> GetTransactionPagedAsync(DateTime initDate, DateTime endDate, int pageNumber, int pageSize)
        {
            string cacheKey = $"report-transactions-{initDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}-{pageNumber}-{pageSize}";

            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<PagedResponseOffset<Transaction>>(cachedData);
            }

            var pagedTransactions = await _transactionRepository.GetByDatePagedAsync(initDate, endDate, pageNumber, pageSize);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(pagedTransactions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheExpiry }
                );

            return pagedTransactions;
        }

        public async Task<Transaction> ProcessTransactionAsync(Transaction transaction)
        {
            return await _transactionRepository.AddAsync(transaction);
        }
    }
}
