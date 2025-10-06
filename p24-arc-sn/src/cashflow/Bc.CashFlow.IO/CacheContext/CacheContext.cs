using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.CacheContext;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.CacheContext;

public class CacheContext : ICacheContext
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<CacheContext> _logger;
	private readonly IServiceProvider _serviceProvider;

	public CacheContext(
		ILogger<CacheContext> logger,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	public ICacheCollection<IUser> User => _serviceProvider.GetService<ICacheCollection<IUser>>()!;
	public ICacheCollection<IAccountType> AccountType => _serviceProvider.GetService<ICacheCollection<IAccountType>>()!;
	public ICacheCollection<IAccount> Account => _serviceProvider.GetService<ICacheCollection<IAccount>>()!;
	public ICacheCollection<ITransaction> Transaction => _serviceProvider.GetService<ICacheCollection<ITransaction>>()!;
}
