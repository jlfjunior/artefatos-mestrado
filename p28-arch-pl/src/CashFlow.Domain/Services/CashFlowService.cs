using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Exceptions;
using CashFlow.Domain.Interfaces;

namespace CashFlow.Domain.Services;

public class CashFlowService : ICashFlowService
{
    private readonly ICashFlowRepository _cashFlowRepository;

    public CashFlowService(ICashFlowRepository cashFlowRepository)
    {
        _cashFlowRepository = cashFlowRepository;
    }

    public async Task<CashFlowDailyAggregate?> RegisterNewAggregate(Guid accountId)
    {
        var cashFlow = await _cashFlowRepository.GetCurrentCashByAccountId(accountId);

        await _cashFlowRepository.Save(cashFlow!);

        return cashFlow;
    }

    public async Task<Transaction?> AddTransaction(Guid accountId, decimal amount, TransactionType type)
    {
        var cashFlow = await _cashFlowRepository.GetCurrentCashByAccountId(accountId);

        if (cashFlow == null) throw new CashFlowNotFoundException($"Cash flow with ID {accountId} was not found.");

        var transaction = new Transaction(accountId, amount, type);

        cashFlow.AddTransaction(transaction);

        await _cashFlowRepository.Save(cashFlow);

        return transaction;
    }

    public async Task<Transaction?> ReverseTransaction(Guid transactionId)
    {
        var cashFlow = await _cashFlowRepository.GetByTransactionId(transactionId);
        if (cashFlow == null)
            throw new TransactionNotFoundException($"Transaction with ID {transactionId} was not found.");

        var newTransaction = cashFlow.ReverseTransaction(transactionId);
        await _cashFlowRepository.Save(cashFlow);

        return newTransaction;
    }
}