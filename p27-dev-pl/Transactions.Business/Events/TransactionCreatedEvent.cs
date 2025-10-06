using Foundation.Business.DomainEvents;
using Transactions.Business.Model;

namespace Transactions.Business.Events
{

    public record TransactionCreatedEvent(Guid TransactionId, string Description, double Value, DateTime Date, TransactionType Type) : EventBase
    {
        
    }
}