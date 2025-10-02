namespace Domain.DTOs.Consolidation;
public class ConsolidationDTO
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal TotalDebitAmount { get; set; }
}