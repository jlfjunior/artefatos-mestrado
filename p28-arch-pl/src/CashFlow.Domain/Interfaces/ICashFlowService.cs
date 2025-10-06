using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Interfaces;

public interface ICashFlowService
{
    Task<CashFlowDailyAggregate?> RegisterNewAggregate(Guid accountId);
    Task<Transaction?> AddTransaction(Guid accountId, decimal amount, TransactionType type);
    Task<Transaction?> ReverseTransaction(Guid transactionId);
}