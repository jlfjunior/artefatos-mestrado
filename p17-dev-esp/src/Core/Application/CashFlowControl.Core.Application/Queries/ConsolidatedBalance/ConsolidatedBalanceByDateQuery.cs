using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using Flunt.Notifications;
using MediatR;

namespace CashFlowControl.Core.Application.Queries
{
    public class ConsolidatedBalanceByDateQuery : Notifiable<Notification>, IRequest<Result<ConsolidatedBalance?>>
    {
        public DateTime Date;

        public ConsolidatedBalanceByDateQuery(DateTime date)
        {
            Date = date;
        }
    }
}
