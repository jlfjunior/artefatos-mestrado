using StackExchange.Redis;

namespace Bc.CashFlow.Domain.CacheContext;

public interface ICacheConnection
{
	IConnectionMultiplexer Connect();
}