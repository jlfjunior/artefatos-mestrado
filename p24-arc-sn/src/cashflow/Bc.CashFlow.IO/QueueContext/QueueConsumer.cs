using System.Linq.Expressions;
using System.Text;
using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.QueueContext;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Bc.CashFlow.IO.QueueContext;

public class QueueConsumer : IQueueConsumer
{
	private readonly QueueConfig _config;
	private readonly IConnection _connection;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<QueueConsumer> _logger;

	public QueueConsumer(
		ILogger<QueueConsumer> logger,
		IConnection connection,
		QueueConfig config)
	{
		_logger = logger;
		_connection = connection;
		_config = config;
	}

	public async Task QueuePooling(
		string queue,
		Action<string> messageReception,
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
					
					BasicGetResult? result = channel.BasicGet(
						queue,
						autoAck: false);

					while (result is not null)
					{
						byte[] body = result.Body.ToArray();
						string message = Encoding.UTF8.GetString(body);

						messageReception(message);
						
						channel.BasicAck(
							deliveryTag: result.DeliveryTag,
							multiple: false);
						
						result = channel.BasicGet(
							queue,
							autoAck: false);
					}
				}
			},
			cancellationToken);
	}
}
