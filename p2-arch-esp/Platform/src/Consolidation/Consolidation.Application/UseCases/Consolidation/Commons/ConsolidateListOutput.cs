namespace Consolidation.Application.UseCases.Consolidation.Commons;

public class ConsolidateListOutput
{
    public List <ConsolidateOutput> Data { get; set; }

    public ConsolidateListOutput(List<ConsolidateOutput> data)
    {
        Data = data;
    }
}
