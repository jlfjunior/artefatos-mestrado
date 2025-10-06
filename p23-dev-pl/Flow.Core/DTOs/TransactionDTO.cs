using Flow.Core.Enums;

namespace Flow.Core.DTOs
{
    public class TransactionDTO
    {
        public decimal Value { get; set; }
        public TypeTransaction Type { get; set; }
    }
}
