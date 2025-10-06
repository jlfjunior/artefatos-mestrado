using RabbitMQ.Client;

namespace Bc.CashFlow.Domain.QueueContext;

public interface IQueueConnectionFactory
{
	IConnection CreateConnection();
}