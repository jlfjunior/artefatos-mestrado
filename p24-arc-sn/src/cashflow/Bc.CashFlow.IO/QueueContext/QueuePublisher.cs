using System.Text;
using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.QueueContext;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Bc.CashFlow.IO.QueueContext;

public class QueuePublisher : IQueuePublisher
{
	private readonly QueueConfig _config;
	private readonly IConnection _connection;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<QueuePublisher> _logger;

	public QueuePublisher(
		ILogger<QueuePublisher> logger,
		IConnection connection,
		QueueConfig config)
	{
		_logger = logger;
		_connection = connection;
		_config = config;
	}

	public async Task PublishMessage(
		string message,
		string queue,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		await Task.Run(
			() =>
			{
				// ReSharper disable once ConvertToUsingDeclaration
				using (IModel? channel = _connection.CreateModel())
				{
					channel.QueueDeclare(
						queue,
						false,
						false,
						false,
						null);

					byte[] body = Encoding.UTF8.GetBytes(message);
					IBasicProperties basicProperties = channel.CreateBasicProperties();

					basicProperties.Persistent = true;

					channel.BasicPublish(
						_config.Exchange,
						queue,
						basicProperties,
						body);
				}
			},
			cancellationToken);
	}
}