using Domain.DTOs.Consolidation;
using MediatR;

namespace Application.Consolidation.Consolidation.Query.GetAllConsolidations;

public class GetAllConsolidationsQuery : IRequest<List<ConsolidationDTO>>
{
}