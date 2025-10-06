using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Interfaces;
using CashFlow.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace CashFlow.Infrastructure.Persistence.Repositories;

public class MongoCashFlowRepository : ICashFlowRepository
{
    private readonly IMongoCollection<CashFlowDailyAggregate> _cashFlowCollection;

    private readonly ILogger<MongoCashFlowRepository> _logger;

    static MongoCashFlowRepository()
    {
        ConfigureMongoDBMapping();
    }

    public MongoCashFlowRepository(IMongoDatabase database, ILogger<MongoCashFlowRepository> logger)
    {
        _cashFlowCollection = database.GetCollection<CashFlowDailyAggregate>("cashflows");
        _logger = logger;
    }

    public async Task<CashFlowDailyAggregate?> GetCurrentCashByAccountId(Guid accountId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var filterToday = Builders<CashFlowDailyAggregate>.Filter.Eq(f => f.AccountId, accountId) &
                          Builders<CashFlowDailyAggregate>.Filter.Eq(d => d.Date, today);

        var filterLastTransactionDay =
            Builders<CashFlowDailyAggregate>.Filter.Eq(d => d.AccountId, accountId) &
            Builders<CashFlowDailyAggregate>.Filter.Gt(d => d.Transactions.Count, 0);

        var resultToday = await _cashFlowCollection.Find(filterToday).FirstOrDefaultAsync();

        if (resultToday != null) return resultToday;

        var resultLastTransactionDay = await _cashFlowCollection.Find(filterLastTransactionDay)
            .SortByDescending(d => d.Date)
            .FirstOrDefaultAsync();

        if (resultLastTransactionDay == null) return null;

        return new CashFlowDailyAggregate(Guid.NewGuid(), accountId, today, resultLastTransactionDay.CurrentBalance);
    }

    public async Task<(List<CashFlowDailyAggregate?> response, long totalItemCount, int totalPages)>
        GetByAccountIdAndDateRange(Guid accountId, DateOnly startDate, DateOnly endDate, int pageNumber = 1,
            int pageSize = 50)
    {
        var skipCount = (pageNumber - 1) * pageSize;

        var filter = Builders<CashFlowDailyAggregate>.Filter.And(
            Builders<CashFlowDailyAggregate>.Filter.Eq(d => d.AccountId, accountId),
            Builders<CashFlowDailyAggregate>.Filter.Gte(d => d.Date, startDate),
            Builders<CashFlowDailyAggregate>.Filter.Lte(d => d.Date, endDate)
        );

        var totalItemCount = await _cashFlowCollection.CountDocumentsAsync(filter);

        var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);


        var response = await _cashFlowCollection.Find(filter)
            .SortByDescending(d => d.Date)
            .Skip(skipCount)
            .Limit(pageSize)
            .ToListAsync();

        return (response, totalItemCount, totalPages);
    }

    public async Task<CashFlowDailyAggregate?> GetByTransactionId(Guid transactionId)
    {
        var cashFlow = await _cashFlowCollection.Find(cf => cf.Transactions.Any(t => t.Id == transactionId))
            .FirstOrDefaultAsync();
        return cashFlow;
    }

    public async Task Save(CashFlowDailyAggregate cashFlowDaily)
    {
        var existingCashFlow = await GetById(cashFlowDaily.Id);
        if (existingCashFlow != null)
            await _cashFlowCollection.ReplaceOneAsync(cf => cf.Id == cashFlowDaily.Id, cashFlowDaily);

        else
            await _cashFlowCollection.InsertOneAsync(cashFlowDaily);
    }

    private async Task<CashFlowDailyAggregate?> GetById(Guid cashFlowDailyId)
    {
        return await _cashFlowCollection.Find(d => d.Id == cashFlowDailyId)
            .FirstOrDefaultAsync();
    }

    private static void ConfigureMongoDBMapping()
    {
        BsonClassMap.RegisterClassMap<Money>(cm => { cm.MapProperty(m => m.Amount); });

        BsonClassMap.RegisterClassMap<Transaction>(cm =>
        {
            cm.MapProperty(t => t.Id);
            cm.MapProperty(t => t.CashFlowId);
            cm.MapProperty(t => t.AmountVO);
            cm.MapProperty(t => t.Timestamp);
            cm.MapProperty(t => t.Type);
        });

        BsonClassMap.RegisterClassMap<CashFlowDailyAggregate>(cm =>
        {
            cm.MapIdProperty(cf => cf.Id);
            cm.MapProperty(cf => cf.Transactions);
        });
    }
}