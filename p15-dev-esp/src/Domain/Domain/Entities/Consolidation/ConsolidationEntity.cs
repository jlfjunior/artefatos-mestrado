using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.Consolidation;
public class ConsolidationEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal TotalDebitAmount { get; set; }
}
