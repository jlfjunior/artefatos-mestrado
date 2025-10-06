using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Flow.Core.Entities
{
    public class DailyBalance
    {
        [BsonId] // Define como identificador no MongoDB
        [BsonRepresentation(BsonType.ObjectId)] // Garante que o MongoDB lide com ObjectId corretamente
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("date")] // Define o nome do campo no MongoDB
        public DateTime Date { get; set; }

        [BsonElement("totalBalance")]
        public decimal TotalBalance { get; set; }
    }
}
