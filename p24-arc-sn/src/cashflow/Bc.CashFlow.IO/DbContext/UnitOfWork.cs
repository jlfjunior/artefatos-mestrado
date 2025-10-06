using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.DbContext;

public sealed class UnitOfWork : IUnitOfWork
{
	private readonly DbConnection _dbConnection;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<UnitOfWork> _logger;
	private readonly IServiceProvider _serviceProvider;
	private DbTransaction? _dbTransaction;
	private bool _disposed;

	public UnitOfWork(
		ILogger<UnitOfWork> logger,
		DatabaseConfig config,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
		_dbConnection = new SqlConnection(config.ConnectionString);
	}

	// ReSharper disable once ConvertToAutoPropertyWhenPossible
	public DbConnection Connection => _dbConnection;

	// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
	public DbTransaction? Transaction => _dbTransaction;

	public IUserRepository UserRepository => _serviceProvider.GetService<IUserRepository>()!;
	public IAccountTypeRepository AccountTypeRepository => _serviceProvider.GetService<IAccountTypeRepository>()!;
	public IAccountRepository AccountRepository => _serviceProvider.GetService<IAccountRepository>()!;
	public ITransactionRepository TransactionRepository => _serviceProvider.GetService<ITransactionRepository>()!;
	public IDailyReportRepository DailyReportRepository => _serviceProvider.GetService<IDailyReportRepository>()!;

	public async Task BeginTransactionAsync()
	{
		if (_dbTransaction is not null)
		{
			throw new ConnectionInUseByOtherTransactionException();
		}

		if (!ConnectionState.Open.Equals(_dbConnection.State))
		{
			await _dbConnection.OpenAsync();
		}

		_dbTransaction = await _dbConnection.BeginTransactionAsync();
	}

	public async Task CommitAsync()
	{
		if (_dbTransaction is null)
		{
			throw new ConnectionWithoutTransactionException();
		}

		await _dbTransaction.CommitAsync();
		await _dbTransaction.DisposeAsync();

		_dbTransaction = null;
	}

	public async Task RollbackAsync()
	{
		await _dbTransaction?.RollbackAsync()!;
	}

	[SuppressMessage(
		"Usage",
		"CA1816:Dispose methods should call SuppressFinalize")]
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
			_dbTransaction?.Dispose();
			_dbConnection.Dispose();
		}

		_disposed = true;
	}

	~UnitOfWork()
	{
		Dispose(false);
	}
}