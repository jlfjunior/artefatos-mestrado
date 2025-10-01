using CashFlow.DailyConsolidated.Domain.Entities;
using CashFlow.DailyConsolidated.Domain.Interfaces;
using CashFlow.Entries.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CashFlow.DailyConsolidated.Application.Services
{
    public class DailyConsolidatedService : IDailyConsolidatedService
    {
        private readonly IEntryRepository _entryRepository;
        private readonly ICacheRepository _cacheRepository;
        private readonly ILogger<DailyConsolidatedService> _logger;

        public DailyConsolidatedService(
            IEntryRepository entryRepository,
            ICacheRepository cacheRepository,
            ILogger<DailyConsolidatedService> logger)
        {
            _entryRepository = entryRepository;
            _cacheRepository = cacheRepository;
            _logger = logger;
        }

        public async Task<DailyConsolidatedEntity> GetAsync(DateOnly date)
        {
            var entries = await _cacheRepository.GetDailyConsolidatedByDate(date);

            if (entries != null)
            {
                _logger.LogInformation($"Cache hit for date {date}");
                return entries;
            }

            _logger.LogInformation($"Cache miss for date {date}, fetching from repository");

            var entriesList = await _entryRepository.GetEntriesByDate(date);

            if (entriesList == null || !entriesList.Any())
            {
                _logger.LogWarning($"No entries found for date {date}");
                return new DailyConsolidatedEntity(date, new List<Entry>());
            }

            var dailyConsolidated = new DailyConsolidatedEntity(date, entriesList.ToList());
            await _cacheRepository.AddDailyConsolidation(dailyConsolidated);

            return dailyConsolidated;
        }
    }
}
