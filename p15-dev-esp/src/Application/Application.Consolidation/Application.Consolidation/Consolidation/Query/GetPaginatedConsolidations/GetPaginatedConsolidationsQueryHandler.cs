using Application.Product.Product.Query.GetPaginatedConsolidations;
using Domain.DTOs;
using Domain.DTOs.Consolidation;
using MediatR;
using Services.Interfaces;

namespace Application.Consolidation.Consolidation.Query.GetPaginatedConsolidations;
public class GetPaginatedProductsQueryHandler(IConsolidationService _service) : IRequestHandler<GetPaginatedConsolidationsQuery, PaginatedResult<ConsolidationDTO>>
{
    public async Task<PaginatedResult<ConsolidationDTO>> Handle(GetPaginatedConsolidationsQuery query, CancellationToken cancellationToken) => await _service.GetPaginated(query.PageNumber, query.PageSize);
}
