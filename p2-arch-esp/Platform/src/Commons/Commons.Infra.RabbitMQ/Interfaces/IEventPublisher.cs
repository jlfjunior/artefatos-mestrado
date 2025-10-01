using Commons.Infra.RabbitMQ.Events;

namespace Commons.Infra.RabbitMQ.Interfaces;

public interface IEventPublisher
{
    void PublishCreatedTransaction(CreatedTransactionEvent evt);
}
