using System.Data.Common;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Bc.CashFlow.IO.DbContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

// ReSharper disable once InconsistentNaming
public class IODbContextBuilder : IContextBuilderInstaller
{
	public void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null)
	{
		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
		builder.Services.AddScoped<DbConnection>(
			sp =>
				sp.GetRequiredService<IUnitOfWork>().Connection);
		builder.Services.AddScoped<DbTransaction>(
			sp =>
				sp.GetRequiredService<IUnitOfWork>().Transaction!);
		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
		builder.Services.AddScoped<IAccountRepository, AccountRepository>();
		builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
		builder.Services.AddScoped<IDailyReportRepository, DailyReportRepository>();
	}
}