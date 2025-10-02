using Domain.DTOs.Launch;
using Domain.Models;
using MediatR;

namespace Application.Launch.Launch.Command.UpdateLaunchConsolidation;
public class UpdateLaunchConsolidationCommand : IRequest<ApiResponse>
{
    public List<LaunchDTO> Launches { get; set; } = new List<LaunchDTO>();
}
