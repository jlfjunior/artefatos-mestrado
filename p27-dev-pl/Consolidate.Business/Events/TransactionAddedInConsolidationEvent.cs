using Consolidate.Business.Model;
using Foundation.Business.DomainEvents;

namespace Consolidate.Business.Events
{
    public record TransactionAddedInConsolidationEvent(Guid TransactionId, string Description, double Value, DateTime Date, TransactionType Type) : EventBase { }
}