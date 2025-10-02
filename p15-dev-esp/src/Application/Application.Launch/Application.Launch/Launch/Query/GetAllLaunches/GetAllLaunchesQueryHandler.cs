using Domain.DTOs.Launch;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Launch.Query.GetAllLaunches;
public class GetAllLaunchesQueryHandler (ILaunchService _service) : IRequestHandler<GetAllLaunchesQuery, List<LaunchDTO>>
{
    public async Task<List<LaunchDTO>> Handle(GetAllLaunchesQuery query, CancellationToken cancellationToken) => await _service.GetAll();
}
