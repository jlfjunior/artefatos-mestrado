using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.CacheContext;

[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface ICacheCollection<TContext>
	where TContext : class
{
	string KeyPrefix { get; }

	Task SetVale(
		string key,
		TContext value,
		CancellationToken cancellationToken);

	Task<TContext?> GetValue(
		string key,
		CancellationToken cancellationToken);
}