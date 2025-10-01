using Commons.Infra.RabbitMQ.Events;
using Commons.Infra.RabbitMQ.Handlers;
using Consolidation.Application.Interfaces.Repository;
using Consolidation.Domain.Entity;

namespace Consolidation.Application.Handlers;

public class CreatedTransactionEventHandler : ICreatedTransactionEventHandler
{

    private readonly IConsolidateRepository _repository;

    public CreatedTransactionEventHandler(IConsolidateRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CreatedTransactionEvent evt)
    {
        await _repository.SaveAsync(new Consolidate(evt.MovementId, evt.Description, evt.Value));
    }
}
