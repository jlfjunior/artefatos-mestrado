using Consolidate.Business.Model;
using Foundation.Business.DomainEvents;

namespace Consolidate.Business.Events
{
    public record TransactionConsolidationCreatedEvent(Guid TransactionId, string Description, double Value, DateTime Date, TransactionType Type) : EventBase { }
}