using Repositories.Repositories.Generic.GenericMongoDB;

namespace Domain.Entities.Consolidation;

public interface IConsolidationRepository : IMongoDBRepository<ConsolidationEntity>
{
}