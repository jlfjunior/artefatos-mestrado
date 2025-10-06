using MongoDB.Bson.Serialization.Attributes;
using Transactions.Business.Model;

namespace Transactions.Data.Mapping
{
    public class TransactionMongoMap
    {
        [BsonId()]
        public Guid Id;
        [BsonElement("Description")]
        public string Description;
        [BsonElement("Value")]
        public double Value;
        [BsonElement("Type")]
        public TransactionType Type;
        [BsonElement("Date")]
        public DateTime Date;

    }
}
