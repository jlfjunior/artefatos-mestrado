using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections;
using MongoDB.Driver;

namespace FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Contexts.Base
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;
        public MongoClient MongoClient { get; set; }

        public MongoDBContext(string connection, string database)
        {
            MongoClient = new MongoClient(MongoClientSettings.FromConnectionString(connection));

            if (MongoClient != null)
                _database = MongoClient.GetDatabase(database);
        }

        public IMongoCollection<T> GetMongoCollection<T>() where T : IMongoEntity
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }

        public IMongoDatabase GetMongoDatabase()
        {
            return _database;
        }
    }
}
