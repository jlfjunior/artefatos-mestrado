using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.QueueContext;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Bc.CashFlow.IO.CacheContext;

public class QueueConnectionFactory : IQueueConnectionFactory
{
	private readonly QueueConfig _config;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<CacheConnection> _logger;

	public QueueConnectionFactory(
		ILogger<CacheConnection> logger,
		QueueConfig config)
	{
		_logger = logger;
		_config = config;
	}

	public IConnection CreateConnection()
	{
		if (_config.HostName is null)
		{
			throw new NullQueueHostNameException();
		}

		ConnectionFactory connectionFactory = new()
		{
			HostName = _config.HostName,
			UserName = _config.UserName,
			Password = _config.Password
		};

		return connectionFactory.CreateConnection();
	}
}