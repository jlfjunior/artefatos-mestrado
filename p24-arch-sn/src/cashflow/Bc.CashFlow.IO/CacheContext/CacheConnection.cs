using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.CacheContext;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Bc.CashFlow.IO.CacheContext;

public class CacheConnection : ICacheConnection
{
	private readonly CacheConfig _config;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<CacheConnection> _logger;

	public CacheConnection(
		ILogger<CacheConnection> logger,
		CacheConfig config)
	{
		_logger = logger;
		_config = config;
	}

	public IConnectionMultiplexer Connect()
	{
		if (_config.Configuration is null)
		{
			throw new NullCacheConfigurationException();
		}

		ConfigurationOptions configuration = ConfigurationOptions.Parse(
			_config.Configuration,
			_config.IgnoreUnknown);

		return ConnectionMultiplexer.Connect(configuration);
	}
}