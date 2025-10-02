using Domain.DTOs.Launch;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Launch.Query.GetAllDailyLaunches;
public class GetAllPendingDailyLaunchesQueryHandler (ILaunchService _service) : IRequestHandler<GetAllPendingDailyLaunchesQuery, List<LaunchDTO>>
{
    public async Task<List<LaunchDTO>> Handle(GetAllPendingDailyLaunchesQuery query, CancellationToken cancellationToken) => await _service.GetAllPendingDaily();
}
