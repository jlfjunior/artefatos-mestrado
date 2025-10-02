using Domain.Models;
using Domain.Models.Launch.Launch;
using MediatR;

namespace Application.Launch.Launch.Command.CreateLaunch;
public class CreateLaunchCommand : IRequest<ApiResponse>
{
    public LaunchTypeEnum LaunchType { get; set; }
    public List<ProductsOrder> ProductsOrder { get; set; } = new List<ProductsOrder>();
}