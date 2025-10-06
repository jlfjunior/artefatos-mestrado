using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Bc.CashFlow.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

public class ServiceContextBuilder : IContextBuilderInstaller
{
	public void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null)
	{
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IAccountTypeService, AccountTypeService>();
		builder.Services.AddScoped<IAccountService, AccountService>();
		builder.Services.AddScoped<ITransactionService, TransactionService>();
		builder.Services.AddScoped<IDailyReportService, DailyReportService>();
	}
}