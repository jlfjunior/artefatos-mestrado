using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Business;

public class DailyReportBusiness : IDailyReportBusiness
{
	private readonly IAccountService _accountService;
	private readonly IDailyReportService _dailyReportService;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DailyReportBusiness> _logger;
	private readonly ITransactionService _transactionService;

	public DailyReportBusiness(
		ILogger<DailyReportBusiness> logger,
		ITransactionService transactionService,
		IAccountService accountService,
		IDailyReportService dailyReportService)
	{
		_logger = logger;
		_transactionService = transactionService;
		_accountService = accountService;
		_dailyReportService = dailyReportService;
	}

	public async Task ConsolidateDailyReport(
		DateTime referenceDay,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IEnumerable<Identity<int>> accountIdsList =
			await _accountService.GetAccountsId(
				cancellationToken);
		TransactionsBalanceReport totalTransactionsBalanceReport =
			await ConsolidateAccountsListBalance(
				accountIdsList,
				referenceDay,
				cancellationToken);

		Identity<int>? totalDailyReport =
			await _dailyReportService.CreateDailyReport(
				null,
				referenceDay,
				totalTransactionsBalanceReport.TotalDebits,
				totalTransactionsBalanceReport.TotalCredits,
				totalTransactionsBalanceReport.TotalFees,
				totalTransactionsBalanceReport.FinalBalance,
				cancellationToken);

		if (totalDailyReport is null) throw new DailyReportCreationReturnedNullIdentityException();
	}

	private async Task<TransactionsBalanceReport> ConsolidateAccountsListBalance(
		IEnumerable<Identity<int>> accountIdsList,
		DateTime referenceDay,
		CancellationToken cancellationToken)
	{
		TransactionsBalanceReport result = new();

		foreach (Identity<int> accountIdentityId in accountIdsList)
		{
			TransactionsBalanceReport accountTransactionsBalanceReport =
				await ConsolidateAccountDailyReport(
					referenceDay,
					accountIdentityId.Value,
					cancellationToken);

			result.AddDebit(accountTransactionsBalanceReport.TotalDebits);
			result.AddCredit(accountTransactionsBalanceReport.TotalCredits);
			result.AddFee(accountTransactionsBalanceReport.TotalFees);
		}

		return result;
	}

	public async Task<TransactionsBalanceReport> ConsolidateAccountDailyReport(
		DateTime referenceDay,
		int accountId,
		CancellationToken cancellationToken)
	{
		IEnumerable<ITransaction> transactionsList =
			await _transactionService.GetTransactionsOnProjectedRepaymentDate(
				accountId,
				referenceDay,
				cancellationToken);

		TransactionsBalanceReport result =
			GetTransactionsBalanceReport(transactionsList);

		Identity<int>? accountDailyReport =
			await _dailyReportService.CreateDailyReport(
				accountId,
				referenceDay,
				result.TotalDebits,
				result.TotalCredits,
				result.TotalFees,
				result.FinalBalance,
				cancellationToken);

		if (accountDailyReport is null) throw new DailyReportCreationReturnedNullIdentityException();

		return result;
	}

	public static TransactionsBalanceReport GetTransactionsBalanceReport(
		IEnumerable<ITransaction> transactionsList)
	{
		TransactionsBalanceReport result = new();

		foreach (ITransaction transaction in transactionsList)
		{
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (transaction.TransactionType)
			{
				case TransactionType.Debit:
					result.AddDebit(transaction.Amount);
					break;
				case TransactionType.Credit:
					result.AddCredit(transaction.Amount);
					break;
			}

			result.AddFee(transaction.TransactionFee ?? 0);
		}

		return result;
	}

	public async Task<IEnumerable<IDailyReport>> GetDailyReports(
		DateTime? referenceDateSince,
		DateTime? referenceDateUntil,
		CancellationToken cancellationToken)
	{
		IEnumerable<Identity<int>> dailyReportsIdsList =
			await GetDailyReportsIdList(
				referenceDateSince,
				referenceDateUntil,
				cancellationToken);
		IEnumerable<IDailyReport> dailyReportsList =
			await GetDailyReports(
				dailyReportsIdsList,
				cancellationToken);

		return dailyReportsList;
	}

	public async Task<IEnumerable<Identity<int>>> GetDailyReportsIdList(
		DateTime? referenceDateSince,
		DateTime? referenceDateUntil,
		CancellationToken cancellationToken)
	{
		return await _dailyReportService.GetDailyReportsId(
			referenceDateSince,
			referenceDateUntil,
			cancellationToken);
	}

	public async Task<IEnumerable<IDailyReport>> GetDailyReports(
		IEnumerable<Identity<int>> dailyReportsIdsList,
		CancellationToken cancellationToken)
	{
		List<IDailyReport> result = [];

		foreach (Identity<int> dailyReportIdentity in dailyReportsIdsList)
		{
			IDailyReport dailyReport = await _dailyReportService.GetDailyReport(
				dailyReportIdentity.Value,
				cancellationToken);

			result.Add(dailyReport);
		}

		return result;
	}

	public struct TransactionsBalanceReport
	{
		public decimal TotalDebits { get; private set; } = 0m;
		public decimal TotalCredits { get; private set; } = 0m;
		public decimal TotalFees { get; private set; } = 0m;
		public decimal BalanceBeforeFees => TotalCredits - TotalDebits;
		public decimal FinalBalance => BalanceBeforeFees - TotalFees;

		public TransactionsBalanceReport()
		{
		}

		public TransactionsBalanceReport(
			decimal debits,
			decimal credits,
			decimal fees)
		{
			TotalDebits = debits;
			TotalCredits = credits;
			TotalFees = fees;
		}

		public void AddDebit(
			decimal value)
		{
			TotalDebits += value;
		}

		public void AddCredit(
			decimal value)
		{
			TotalCredits += value;
		}

		public void AddFee(
			decimal value)
		{
			TotalFees += value;
		}
	}
}
