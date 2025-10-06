using Consolidate.Business.Model;
using MediatR;

namespace Consolidate.Business.UseCase.AddConsolidateTransaction
{
    public record AddConsolidateTransactionInputDto(Guid TransactionId, string Description, double Value, DateTime Date, TransactionType Type) : IRequest { }
}
