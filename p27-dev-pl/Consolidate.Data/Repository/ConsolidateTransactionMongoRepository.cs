using AutoMapper;
using Consolidate.Business.Contracts;
using Consolidate.Business.Model;
using Consolidate.Data.Mapping;
using MongoDB.Driver;

namespace Consolidate.Data.Repository
{
    public class ConsolidateTransactionMongoRepository : IConsolidateTransactionRepository
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<ConsolidateTransactionMap> _collection;
        private readonly IMapper _mapper;

        public ConsolidateTransactionMongoRepository(IMongoDatabase mongoDatabase, IMapper mapper)
        {
            _mapper = mapper;
            _mongoDatabase = mongoDatabase;
            _collection = mongoDatabase.GetCollection<ConsolidateTransactionMap>("consolidateTransactions");
        }

        public async Task Create(ConsolidateTransaction consolidateTransaction, CancellationToken cancellationToken)
        {
            var consolidateMap = _mapper.Map<ConsolidateTransactionMap>(consolidateTransaction);
            await _collection.InsertOneAsync(consolidateMap);
            
        }

        public async Task<ConsolidateTransaction?> Get(DateOnly date, CancellationToken cancellationToken)
        {
            var consolidateMap = await _collection.Find(c => c.Date == date).FirstOrDefaultAsync();
            var consolidate = _mapper.Map<ConsolidateTransaction>(consolidateMap);            

            return consolidate;
        }

        public async Task Save(ConsolidateTransaction consolidateTransaction, CancellationToken cancellationToken)
        {
            var consolidateMap = _mapper.Map<ConsolidateTransactionMap>(consolidateTransaction);
            var options = new ReplaceOptions { IsUpsert = true };

            await _collection.ReplaceOneAsync(x => x.Id == consolidateMap.Id, consolidateMap, options, cancellationToken);
        }
    }

    
}
