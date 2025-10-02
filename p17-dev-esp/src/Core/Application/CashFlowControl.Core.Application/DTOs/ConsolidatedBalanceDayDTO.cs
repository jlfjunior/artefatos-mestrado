using CashFlowControl.Core.Domain.Entities;

namespace CashFlowControl.Core.Application.DTOs
{
    public class ConsolidatedBalanceDayDTO
    {
        public ConsolidatedBalance ConsolidatedBalance { get; set; } = new ConsolidatedBalance();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
