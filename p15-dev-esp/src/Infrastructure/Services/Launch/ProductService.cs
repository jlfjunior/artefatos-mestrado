using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Domain.Entities.Launch;
using Domain.Entities.Product;
using Infrastructure.Repositories.UnitOfWork;
using Domain.DTOs.Launch;
using Domain.DTOs;

namespace Application.Authentication.Services;

public class ProductService(IProductRepository _repository, IUnitOfWork _unitOfWork, ILogger<ProductService> _logger) : IProductService
{

    public async Task<ApiResponse> CreateProduct(string name, decimal price, int stock)
    {
        try
        {
            if (price <= 0)
            {
                _logger.LogError("Invalid product price.");
                return new ApiResponse { Message = "Price must be greater than zero.", Success = false };
            }

            var product = new ProductEntity
            {
                Name = name,
                Price = price,
                Stock = stock
            };

            await _repository.AddAsync(product);
            await _unitOfWork.CommitAsync();

            _logger.LogDebug($"Product {product.Name} created successfully.");
            return new ApiResponse { Success = true, Message = "Product created successfully." };
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to create product {name}");
            return new ApiResponse { Success = false, Message = "Failed to create product." };
        }
    }

    public async Task<List<ProductDTO>> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(p => new ProductDTO
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock
        }).ToList();
    }

    public async Task<ProductDTO> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        };
    }

    public async Task<PaginatedResult<ProductDTO>> GetPaginated(int pageNumber, int pageSize)
    {
        var productsAll = await _repository.GetAllAsync();

        var productsPaginated = productsAll.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var paginatedProducts = productsPaginated.Select(p => new ProductDTO
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock
        }).ToList();

        return new PaginatedResult<ProductDTO>(paginatedProducts, productsAll.Count, pageSize, pageNumber);
    }

    public async Task<ApiResponse> UpdateProduct(int id, string name, decimal price, int stock)
    {
        try
        {
            await _repository.UpdateAsync(new ProductEntity
            {
                Id = id,
                Name = name,
                Price = price,
                Stock = stock
            });

            _logger.LogDebug($"Product {name} updated successfully.");
            return new ApiResponse { Success = true, Message = "Product updated successfully." };
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to update product {name}");
            return new ApiResponse { Success = false, Message = "Failed to update product." };
        }
    }

    public async Task<ApiResponse> DeleteProduct(int id)
    {
        try
        {
            await _repository.DeleteAsync(id);

            _logger.LogDebug($"Product {id} deleted successfully.");
            return new ApiResponse { Success = true, Message = "Product deleted successfully." };
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to delete product {id}");
            return new ApiResponse { Success = false, Message = "Failed to delete product." };
        }
    }
}
