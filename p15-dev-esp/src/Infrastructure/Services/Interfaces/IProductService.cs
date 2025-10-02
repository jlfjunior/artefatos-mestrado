using Domain.DTOs;
using Domain.DTOs.Launch;
using Domain.Models;

namespace Services.Interfaces;
public interface IProductService
{
    Task<ApiResponse> CreateProduct(string name, decimal price, int stock);
    Task<ApiResponse> UpdateProduct(int id, string name, decimal price, int stock);
    Task<ApiResponse> DeleteProduct(int id);
    Task<List<ProductDTO>> GetAll();
    Task<ProductDTO> GetById(int id);
    Task<PaginatedResult<ProductDTO>> GetPaginated(int pageNumber, int pageSize);
}