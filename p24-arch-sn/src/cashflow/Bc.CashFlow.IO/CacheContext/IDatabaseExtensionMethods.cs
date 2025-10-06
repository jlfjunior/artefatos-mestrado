using System.Text.Json;
using StackExchange.Redis;

namespace Bc.CashFlow.IO.CacheContext;

// ReSharper disable once InconsistentNaming
public static class IDatabaseExtensionMethods
{
	public static async Task SetValue<TValue>(
		this IDatabase db,
		string prefix,
		string key,
		TValue value,
		CancellationToken cancellationToken)
		where TValue : class
	{
		cancellationToken.ThrowIfCancellationRequested();

		string jsonValue = JsonSerializer.Serialize(value);
		TimeSpan ttl = TimeSpan.FromMinutes(10);

		await db.StringSetAsync(
			$"{prefix}:{key}",
			jsonValue,
			ttl);
	}

	public static async Task<TValue?> GetValue<TValue>(
		this IDatabase db,
		string prefix,
		string key,
		CancellationToken cancellationToken)
		where TValue : class
	{
		cancellationToken.ThrowIfCancellationRequested();

		string? jsonValue = await db.StringGetAsync(
			$"{prefix}:{key}");

		return jsonValue is null
			? null
			: JsonSerializer.Deserialize<TValue>(jsonValue);
	}
}