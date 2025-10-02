using Domain.DTOs.Launch;
using MediatR;

namespace Application.Launch.Launch.Query.GetAllDailyLaunches;
public class GetAllPendingDailyLaunchesQuery : IRequest<List<LaunchDTO>>
{
}