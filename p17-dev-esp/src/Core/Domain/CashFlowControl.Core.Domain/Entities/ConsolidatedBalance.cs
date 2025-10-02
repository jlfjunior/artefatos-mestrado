using System.ComponentModel.DataAnnotations.Schema;

namespace CashFlowControl.Core.Domain.Entities
{
    public class ConsolidatedBalance
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
    }
}
