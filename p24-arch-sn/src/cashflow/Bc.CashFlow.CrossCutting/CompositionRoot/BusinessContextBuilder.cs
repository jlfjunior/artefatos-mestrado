using Bc.CashFlow.Business;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

public class BusinessContextBuilder : IContextBuilderInstaller
{
	public void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null)
	{
		builder.Services.AddScoped<IUserBusiness, UserBusiness>();
		builder.Services.AddScoped<IAccountTypeBusiness, AccountTypeBusiness>();
		builder.Services.AddScoped<IAccountBusiness, AccountBusiness>();
		builder.Services.AddScoped<ITransactionBusiness, TransactionBusiness>();
		builder.Services.AddScoped<IDailyReportBusiness, DailyReportBusiness>();
	}
}