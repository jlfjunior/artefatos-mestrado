using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.User;
using StackExchange.Redis;

namespace Bc.CashFlow.IO.CacheContext;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class UserCacheCollection : BaseCacheCollection<IUser, UserCacheCollection.UserDto>
{
	public UserCacheCollection(
		IDatabase db)
		: base("user", db)
	{
	}

	public record UserDto : IUser
	{
		public required int Id { get; init; }
		public required string Username { get; init; }
		public required DateTime CreatedAt { get; init; }
	}
}