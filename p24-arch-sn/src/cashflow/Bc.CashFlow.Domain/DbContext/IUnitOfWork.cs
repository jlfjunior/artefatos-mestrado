using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;

namespace Bc.CashFlow.Domain.DbContext;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IUnitOfWork : IDisposable
{
	DbConnection Connection { get; }
	DbTransaction? Transaction { get; }
	IUserRepository UserRepository { get; }
	IAccountTypeRepository AccountTypeRepository { get; }
	IAccountRepository AccountRepository { get; }
	ITransactionRepository TransactionRepository { get; }
	IDailyReportRepository DailyReportRepository { get; }

	Task BeginTransactionAsync();
	Task CommitAsync();
	Task RollbackAsync();
}