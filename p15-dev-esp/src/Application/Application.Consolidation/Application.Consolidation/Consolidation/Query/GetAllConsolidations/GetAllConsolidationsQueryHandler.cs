using Domain.DTOs.Consolidation;
using MediatR;
using Services.Interfaces;

namespace Application.Consolidation.Consolidation.Query.GetAllConsolidations;

public class GetAllConsolidationsQueryHandler(IConsolidationService _service) : IRequestHandler<GetAllConsolidationsQuery, List<ConsolidationDTO>>
{
    public async Task<List<ConsolidationDTO>> Handle(GetAllConsolidationsQuery query, CancellationToken cancellationToken) => await _service.GetAll();
}
