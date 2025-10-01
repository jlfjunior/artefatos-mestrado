using Commons.Infra.RabbitMQ.Events;

namespace Commons.Infra.RabbitMQ.Handlers;

public interface ICreatedTransactionEventHandler
{
    Task Handle(CreatedTransactionEvent evt);
}
