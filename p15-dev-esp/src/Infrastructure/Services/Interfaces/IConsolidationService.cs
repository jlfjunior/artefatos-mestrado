using Domain.DTOs;
using Domain.DTOs.Consolidation;
using Domain.DTOs.Launch;

namespace Services.Interfaces;

public interface IConsolidationService
{
    Task<List<LaunchDTO>> CreateConsolidation(List<LaunchDTO> launches);
    Task<List<ConsolidationDTO>> GetAll();
    Task<PaginatedResult<ConsolidationDTO>> GetPaginated(int pageNumber, int pageSize);
}