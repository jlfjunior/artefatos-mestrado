using Domain.DTOs;
using Domain.DTOs.Consolidation;
using MediatR;

namespace Application.Product.Product.Query.GetPaginatedConsolidations;

public class GetPaginatedConsolidationsQuery : IRequest<PaginatedResult<ConsolidationDTO>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
