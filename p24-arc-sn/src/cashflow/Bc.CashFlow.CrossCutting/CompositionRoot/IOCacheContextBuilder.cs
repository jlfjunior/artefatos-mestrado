using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.CacheContext;
using Bc.CashFlow.Domain.User;
using Bc.CashFlow.IO.CacheContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ITransaction = Bc.CashFlow.Domain.Transaction.ITransaction;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

// ReSharper disable once InconsistentNaming
public class IOCacheContextBuilder : IContextBuilderInstaller
{
	public void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null)
	{
		builder.Services.AddSingleton<ICacheConnection, CacheConnection>();
		builder.Services.AddSingleton<IConnectionMultiplexer>(
			sp =>
				sp.GetRequiredService<ICacheConnection>().Connect());
		builder.Services.AddScoped<IDatabase>(
			sp =>
				sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
		builder.Services.AddScoped<ICacheContext, CacheContext>();
		builder.Services.AddScoped<ICacheCollection<IUser>, UserCacheCollection>();
		builder.Services.AddScoped<ICacheCollection<IAccountType>, AccountTypeCacheCollection>();
		builder.Services.AddScoped<ICacheCollection<IAccount>, AccountCacheCollection>();
		builder.Services.AddScoped<ICacheCollection<ITransaction>, TransactionCacheCollection>();
	}
}
