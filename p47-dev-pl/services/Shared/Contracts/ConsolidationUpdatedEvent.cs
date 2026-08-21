using Shared.Enums;

namespace Shared.Contracts
{

    /// <summary>
    /// Evento publicado quando a consolidação é atualizada.
    /// </summary>
    public class ConsolidationUpdatedEvent
    {
        public Guid ConsolidationId { get; set; }
        public Lojista Lojista { get; set; }
        public DateTime ConsolidationDate { get; set; }
        public decimal DebitTotal { get; set; }
        public decimal CreditTotal { get; set; }
        public decimal DailyBalance { get; set; }
        public int ProcessedCount { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    }
}
