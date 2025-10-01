using CashFlow.Application.Transactions.Handlers;
using CashFlow.Domain.Transactions;
using Riok.Mapperly.Abstractions;

namespace CashFlow.Application.Transactions
{
    [Mapper]
    public partial class TransactionMapper
    {
        public partial TransactionEntity ToEntity(CreateTransactionRequest transactionRequest);

        public partial GetTransactionResponse ToResponse(TransactionEntity transactionEntity);

        public partial TransactionCreated ToEvent(TransactionEntity transactionEntity);
    }
}
