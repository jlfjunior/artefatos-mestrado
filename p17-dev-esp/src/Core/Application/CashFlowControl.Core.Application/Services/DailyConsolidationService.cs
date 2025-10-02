using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Application.Queries;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using CashFlowControl.Core.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlowControl.Core.Application.Services
{
    public class DailyConsolidationService : IDailyConsolidationService
    {
        private readonly ITransactionHttpClientService _transactionHttpClient;
        private readonly IConsolidatedBalanceRepository _balanceRepository;
        private readonly ILogger<DailyConsolidationService> _logger;
        private IMediator _mediator;

        public DailyConsolidationService(ITransactionHttpClientService transactionHttpClient, IConsolidatedBalanceRepository balanceRepository, ILogger<DailyConsolidationService> logger, IMediator mediator)
        {
            _transactionHttpClient = transactionHttpClient;
            _balanceRepository = balanceRepository;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task ProcessTransactionAsync(CreateTransactionDTO transaction)
        {
            try
            {
                _logger.LogInformation("Processing transaction {TransactionId}", transaction.Id);

                var resultBalance = await _mediator.Send(new ConsolidatedBalanceByDateQuery(transaction.CreatedAt.Date), CancellationToken.None);
                var balance = resultBalance.Value;

                if (balance == null)
                {
                    balance = new ConsolidatedBalance
                    {
                        Date = transaction.CreatedAt.Date,
                        TotalCredit = transaction.Type.Equals(TransactionType.Credit.ToString()) ? transaction.Amount : 0,
                        TotalDebit = transaction.Type.Equals(TransactionType.Debit.ToString()) ? transaction.Amount : 0,
                        Balance = (transaction.Type.Equals(TransactionType.Credit.ToString()) ? transaction.Amount : 0) - (transaction.Type.Equals(TransactionType.Debit.ToString()) ? transaction.Amount : 0)
                    };

                    await _balanceRepository.CreateBalanceAsync(balance);
                    _logger.LogInformation("Balance for transaction {TransactionId} created successfully", transaction.Id);

                    return;
                }

                if (transaction.Type.Equals(TransactionType.Credit.ToString()))
                    balance.TotalCredit += transaction.Amount;
                else if (transaction.Type.Equals(TransactionType.Debit.ToString()))
                    balance.TotalDebit += transaction.Amount;

                balance.Balance = balance.TotalCredit - balance.TotalDebit;

                await _balanceRepository.UpdateBalanceAsync(balance);
                _logger.LogInformation("Balance for transaction {TransactionId} updated successfully", transaction.Id);
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred while processing the transaction.";
                _logger.LogError(ex, $"{errorMessage} TransactionId: {transaction.Id}");
                throw new ApplicationException(errorMessage);
            }
        }

        public async Task ConsolidateDailyBalanceAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("Consolidating daily balance for date {Date}", date);

                var transactions = await _transactionHttpClient.GetTransactionsByDateAsync(date);
                if (transactions == null || !transactions.Any())
                {
                    _logger.LogInformation("Not found daily balance for {Date}.", date);
                    return;
                }

                decimal totalCredit = transactions.Where(t => t.Type.Equals(TransactionType.Credit.ToString())).Sum(t => t.Amount);
                decimal totalDebit = transactions.Where(t => t.Type.Equals(TransactionType.Debit.ToString())).Sum(t => t.Amount);

                var balance = new ConsolidatedBalance
                {
                    Date = date.Date,
                    TotalCredit = totalCredit,
                    TotalDebit = totalDebit,
                    Balance = totalCredit - totalDebit
                };

                await _balanceRepository.CreateBalanceAsync(balance);

                _logger.LogInformation("Daily balance for {Date} created successfully", date);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while consolidating balance for date {date}";
                _logger.LogError(ex, errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }


        public async Task<Result<ConsolidatedBalanceDayDTO?>> GetConsolidatedBalanceByDateAsync(DateTime date)
        {
            try
            {
                var transactions = await _transactionHttpClient.GetTransactionsByDateAsync(date.Date);

                if (transactions == null || !transactions.Any())
                    return await Task.FromResult(Result<ConsolidatedBalanceDayDTO?>.Success(null));

                var resultBalance = await _mediator.Send(new ConsolidatedBalanceByDateQuery(date), CancellationToken.None);
                var balance = resultBalance.Value;

                if (balance == null)
                    return await Task.FromResult(Result<ConsolidatedBalanceDayDTO?>.Success(null));

                var consolidatedBalanceDay = new ConsolidatedBalanceDayDTO()
                {
                    ConsolidatedBalance = balance,
                    Transactions = transactions
                };

                return Result<ConsolidatedBalanceDayDTO?>.Success(consolidatedBalanceDay);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Result<ConsolidatedBalanceDayDTO?>.Failure(ex.Message));
            }
        }
    }
}
