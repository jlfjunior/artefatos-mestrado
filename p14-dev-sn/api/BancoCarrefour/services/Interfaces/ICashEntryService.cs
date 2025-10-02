using Domain.Models;
using Domain.Models.Requests;
using Domain.Models.Responses;

namespace Services.Interfaces
{
    public interface ICashEntryService
    {
        Task<CreateCashEntryResponse> CreateCashEntryAsync(CreateCashEntryRequest request, CancellationToken cancellationToken);
        Task<PagnatedResult<GetDailyCashEntriesResponse>> GetDailyCashEntriesAsync(GetDailyCashEntriesResquest request, CancellationToken cancellationToken);
    }
}
