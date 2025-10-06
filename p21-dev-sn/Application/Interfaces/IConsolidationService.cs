using Application.DTOs;

namespace Application.Interfaces
{
    public interface IConsolidationService
    {
        Task<ConsolidationReportResponse> GenerateDailyReportAsync(string date);
    }
}