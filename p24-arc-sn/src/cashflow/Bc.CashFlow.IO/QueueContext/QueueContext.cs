using System.Text.Json;
using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.QueueContext;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.QueueContext;

public class QueueContext : IQueueContext
{
	private readonly QueueConfig _config;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<QueueContext> _logger;
	private readonly IQueuePublisher _queuePublisher;
	private readonly IQueueConsumer _queueConsumer;

	public QueueContext(
		ILogger<QueueContext> logger,
		IQueuePublisher queuePublisher,
		IQueueConsumer queueConsumer,
		QueueConfig config)
	{
		_logger = logger;
		_queuePublisher = queuePublisher;
		_queueConsumer = queueConsumer;
		_config = config;
	}

	public async Task PublishNewTransactionToBalance(
		int id,
		CancellationToken cancellationToken)
	{
		string jsonValue =
			JsonSerializer.Serialize(
				new TransactionIdMessage(id));

		await _queuePublisher.PublishMessage(
			jsonValue,
			_config.NewTransactionToBalanceQueue!,
			cancellationToken);
	}

	public async Task IterateNewTransactionToBalanceQueue(
		Action<TransactionIdMessage?> messageReception,
		CancellationToken cancellationToken)
	{
		await _queueConsumer.QueuePooling(
			_config.NewTransactionToBalanceQueue!,
			message =>
			{
				TransactionIdMessage? result =
					JsonSerializer.Deserialize<TransactionIdMessage>(message);

				messageReception(result);
			},
			cancellationToken);
	}
}
