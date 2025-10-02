using Boxflux.Domain.Interfaces;
using MediatR;
using static ConsolidatedDailyCommand;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ConsolidatedDailyCommandHandler : 
    IRequestHandler<GetConsolidatedDailyQuery, decimal>

{
    private readonly IGeralRepository<ConsolidatedDaily> _consolidatedREpository;
    private readonly IGeralRepository<Lauching> _lauchingRepository;

    public ConsolidatedDailyCommandHandler(IGeralRepository<ConsolidatedDaily> consolidatedREpository, IGeralRepository<Lauching> lauchingRepository)
    {
        _consolidatedREpository = consolidatedREpository;
        _lauchingRepository = lauchingRepository;
    }
    public async Task<decimal> Handle(GetConsolidatedDailyQuery request, CancellationToken cancellationToken)
    {
        var lauchings = await _lauchingRepository.GetAllByDateAsync(request.Date);
        var balance = lauchings.Sum(l => l.Value);

        var consolidatedDaily = new ConsolidatedDaily
        {
            DateConsolidate = DateTime.UtcNow,
            
            Balance = balance
        };

        await _consolidatedREpository.AddOrUpdateAsync(consolidatedDaily.Id, consolidatedDaily);

        return balance;
    }
}
