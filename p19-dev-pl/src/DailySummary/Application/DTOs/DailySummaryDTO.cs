namespace Application.DTOs;

public class DailySummaryDTO
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal Balance { get; set; }
}