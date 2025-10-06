using AutoMapper;
using MongoDB.Driver;
using Transactions.Business.Contracts;
using Transactions.Business.Model;
using Transactions.Data.Mapping;

namespace Transactions.Data.Repository
{
    public class TransactionMongoRepository : ITransactionRepository
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<TransactionMongoMap> _collection;
        private readonly IMapper _mapper;
        public TransactionMongoRepository(IMongoDatabase mongoDatabase, IMapper mapper)
        {
            _mapper = mapper;
            _mongoDatabase = mongoDatabase;
            _collection = mongoDatabase.GetCollection<TransactionMongoMap>("transactions");
        }
        public async Task Save(Transaction transaction, CancellationToken cancellationToken)
        {
            var transactionMap = _mapper.Map<TransactionMongoMap>(transaction);
            //var options = new ReplaceOptions { IsUpsert = true };

            await _collection.InsertOneAsync(transactionMap, cancellationToken);
        }
    }
}
