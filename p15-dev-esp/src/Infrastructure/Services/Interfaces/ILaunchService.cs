using Domain.DTOs.Launch;
using Domain.Models;
using Domain.Models.Launch.Launch;

namespace Services.Interfaces;

public interface ILaunchService
{
    Task<ApiResponse> CreateLaunch(LaunchTypeEnum launchType, List<ProductsOrder> productsOrder);
    Task<List<LaunchDTO>> GetAll();
    Task<List<LaunchDTO>> GetAllPendingDaily();
    Task<ApiResponse> UpdateLaunchConsolidation(List<LaunchDTO> launches);
}
