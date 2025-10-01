using Consolidation.Application.UseCases.Consolidation.Commons;
using MediatR;

namespace Consolidation.Application.UseCases.Consolidation.Get;

public interface IConsolidate : IRequestHandler<GetConsolidateInput, ConsolidateListOutput>
{
}
