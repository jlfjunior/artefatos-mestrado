using Domain.DTOs.Launch;
using MediatR;

namespace Application.Launch.Launch.Query.GetAllLaunches;
public class GetAllLaunchesQuery : IRequest<List<LaunchDTO>>
{
}