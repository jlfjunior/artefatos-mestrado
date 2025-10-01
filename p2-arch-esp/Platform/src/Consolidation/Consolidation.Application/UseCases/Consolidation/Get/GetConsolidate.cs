using Consolidation.Application.Interfaces.Repository;
using Consolidation.Application.UseCases.Consolidation.Commons;

namespace Consolidation.Application.UseCases.Consolidation.Get;

public class GetConsolidate : IConsolidate
{
    private readonly IConsolidateRepository _repository;

    public GetConsolidate(IConsolidateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConsolidateListOutput> Handle(GetConsolidateInput request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAllAsync();
        var listOutput = entities.Select(x => new ConsolidateOutput(x.Id, x.Description, x.Value)).ToList();

        return new ConsolidateListOutput(listOutput);
    }
}
