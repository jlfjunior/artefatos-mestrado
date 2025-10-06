using MongoDB.Bson.Serialization.Attributes;

namespace FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections
{
    public interface IMongoEntity
    {
        [BsonId]
        string Id { get; set; }

        [BsonIgnore]
        string Name { get; }
    }
}
