using Domain.DTOs.Launch;
using MediatR;

namespace Application.Consolidation.Consolidation.Command.CreateConsolidation;

public class CreateConsolidationCommand : IRequest<List<LaunchDTO>>
{
    public List<LaunchDTO> Launches { get; set; } = new List<LaunchDTO>();
}