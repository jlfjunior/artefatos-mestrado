namespace CashFlowControl.Core.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
