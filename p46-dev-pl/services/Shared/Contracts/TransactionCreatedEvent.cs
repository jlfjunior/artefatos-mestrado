using Shared.Enums;

namespace Shared.Contracts
{
    /// <summary>
    /// Evento publicado quando uma transação é criada.
    /// Consumido pelo serviço de Consolidação.
    /// </summary>
    public class TransactionCreatedEvent
    {
        public Guid TransactionId { get; set; }
        public Lojista Lojista { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = null!; // "Debit" ou "Credit"
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    }
}
