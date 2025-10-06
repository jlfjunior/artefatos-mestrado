namespace Bc.CashFlow.Domain.CacheContext;

public class NullCacheConfigurationException : Exception
{
	public NullCacheConfigurationException() : base("The cache configuration cannot be null.")
	{
	}
}