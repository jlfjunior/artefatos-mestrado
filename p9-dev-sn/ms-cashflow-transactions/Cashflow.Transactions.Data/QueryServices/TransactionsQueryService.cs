using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Data.Configs;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cashflow.Transactions.Data.QueryServices
{
    public class TransactionsQueryService : ITransactionsQueryService
    {
        private readonly IMongoCollection<Transaction> _transactionCollection;
        private readonly ILogger<TransactionsQueryService> _logger;

        public TransactionsQueryService(IOptions<CashflowDbConfig> options, ILogger<TransactionsQueryService> logger)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase(options.Value.DatabaseName);
            _transactionCollection = database.GetCollection<Transaction>(options.Value.TransactionsCollection);
            _logger = logger;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsSummaryAsync()
        {
            _logger.LogInformation("Getting transactions summary...");

            var filter = Builders<Transaction>.Filter.Gte(x => x.Date, DateTime.UtcNow.AddDays(-30));

            return await _transactionCollection.Find(filter).Sort(Builders<Transaction>.Sort.Descending(t => t.Date)).ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(string type, string category, string search, int page, int pageSize)
        {
            _logger.LogInformation("Getting transactions...");

            var filter = Builders<Transaction>.Filter.Empty;

            if (!string.IsNullOrEmpty(type))
                filter &= Builders<Transaction>.Filter.Eq(t => t.Type, Enum.Parse(typeof(ETransactionType), type));

            if (!string.IsNullOrEmpty(category))
                filter &= Builders<Transaction>.Filter.Eq(x => x.Category, category);

            if (!string.IsNullOrEmpty(search))
                filter &= Builders<Transaction>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(search));

            return await _transactionCollection.Find(filter).Skip((page - 1) * pageSize).Limit(pageSize).Sort(Builders<Transaction>.Sort.Descending(t => t.Date)).ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(string id)
        {
            _logger.LogInformation("Getting transaction by ID...");

            var filter = Builders<Transaction>.Filter.Eq(t => t.Id, id);
            return await _transactionCollection.Find(filter).FirstAsync();
        }

        public async Task<long> GetTransactionsTotalItemsAsync()
        {
            return await _transactionCollection.CountDocumentsAsync(Builders<Transaction>.Filter.Empty);
        }

        public async Task CreateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Creating transaction...");

            await _transactionCollection.InsertOneAsync(transaction);
        }

        public async Task MassiveCreateTransactionsAsync(List<Transaction> transactions)
        {
            _logger.LogInformation("Massive creating transactions...");

            await _transactionCollection.InsertManyAsync(transactions);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Updating transaction...");

            var filter = Builders<Transaction>.Filter.Eq(t => t.Id, transaction.Id);
            var update = Builders<Transaction>.Update
                .Set(t => t.Description, transaction.Description)
                .Set(t => t.Type, transaction.Type)
                .Set(t => t.Category, transaction.Category)
                .Set(t => t.Amount, transaction.Amount)
                .Set(t => t.Date, transaction.Date)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            await _transactionCollection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Deleting transaction...");

            var filter = Builders<Transaction>.Filter.Eq(t => t.Id, transaction.Id);
            await _transactionCollection.DeleteOneAsync(filter);
        }
    }
}
