using MongoDB.Driver;
using Domain.Entities.Consolidation;
using Repositories.Repositories.Generic.GenericMongoDB;

namespace Infrastructure.Repositories.Repositories.Consolidation;
public class ConsolidationRepository : MongoDBRepository<ConsolidationEntity>, IConsolidationRepository
{
    public ConsolidationRepository(IMongoDatabase database, string collectionName) : base(database, collectionName)
    {
    }
}
