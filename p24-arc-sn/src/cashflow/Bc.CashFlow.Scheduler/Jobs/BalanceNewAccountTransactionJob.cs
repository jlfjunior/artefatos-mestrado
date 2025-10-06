using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.QueueContext;
using Bc.CashFlow.Domain.Transaction;

namespace Bc.CashFlow.Scheduler.Jobs;

public class BalanceNewAccountTransactionJob : IJob
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<BalanceNewAccountTransactionJob> _logger;
	private readonly IServiceProvider _serviceProvider;

	public BalanceNewAccountTransactionJob(
		ILogger<BalanceNewAccountTransactionJob> logger,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	public void Run()
	{
		// ReSharper disable once ConvertToUsingDeclaration
		using (IServiceScope scope = _serviceProvider.CreateScope())
		{
			IQueueContext q = scope.ServiceProvider.GetRequiredService<IQueueContext>();
			ITransactionBusiness transactionBusiness = scope.ServiceProvider.GetRequiredService<ITransactionBusiness>();

			q.IterateNewTransactionToBalanceQueue(
					message =>
					{
						if (message is null)
						{
							_logger.LogError("The TransactionIdMessage was null.");
						}
						else
						{
							transactionBusiness.UpdateAccountBalance(
									message.TransactionId,
									CancellationToken.None)
								.GetAwaiter()
								.GetResult();
						}
					},
					CancellationToken.None)
				.GetAwaiter()
				.GetResult();
		}
	}
}
