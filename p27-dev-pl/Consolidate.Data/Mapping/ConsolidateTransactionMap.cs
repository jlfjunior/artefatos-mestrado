using Consolidate.Business.Model;
using MongoDB.Bson.Serialization.Attributes;

namespace Consolidate.Data.Mapping
{
    public  class ConsolidateTransactionMap
    {
        [BsonId()]
        public Guid Id;
        [BsonElement("Date")]
        public DateOnly Date;
        [BsonElement("Transactions")]
        public IEnumerable<Transaction> Transactions;

    }
}
