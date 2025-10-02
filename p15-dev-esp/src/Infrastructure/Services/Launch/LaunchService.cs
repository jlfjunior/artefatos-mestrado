using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Domain.Models.Launch.Launch;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Launch;
using Domain.DTOs.Launch;
using Domain.Entities.Product;
using Domain.Entities.LaunchProduct;
using Infrastructure.Repositories.UnitOfWork;
using Infrastructure.Persistence.Data;

namespace Application.Authentication.Services;

public class LaunchService(
        ILaunchRepository _repository,
        IProductRepository _productRepository,
        ILaunchProductRepository _launchProductRepository,
        IUnitOfWork _unitOfWork,
        ILogger<LaunchService> _logger,
        ApplicationDbContext _context) : ILaunchService
{

    public async Task<ApiResponse> CreateLaunch(LaunchTypeEnum launchType, List<ProductsOrder> productsOrder)
    {
        try
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var launch = await GenerateLaunch(launchType, productsOrder);

                if (launch != null)
                {
                    var result = await GenerateLaunchProduct(productsOrder, launch);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return result;
                }
            }

            return new ApiResponse { Success = true, Message = "Launch created successfully." };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "An error occurred while creating a launch.");
            return new ApiResponse { Success = false, Message = "An error occurred while creating a launch." };
        }
    }

    public async Task<List<LaunchDTO>> GetAll()
    {
        var launches = await _repository.GetAllAsync();

        return launches.Select(l => new LaunchDTO
        {
            Id = l.Id,
            Amount = l.Amount,
            LaunchType = l.LaunchType,
            CreationDate = l.CreationDate,
            Status = l.Status,
            ModificationDate = l.ModificationDate,
            ProductsOrder = l.LaunchProducts.Select(lp => new ProductsOrderDTO
            {
                ProductId = lp.ProductId,
                Quantity = lp.ProductQuantity,
                Price = lp.ProductPrice
            }).ToList()
        }).ToList();
    }

    public async Task<List<LaunchDTO>> GetAllPendingDaily()
    {
        var launches = await _repository.GetAllAsync();

        return launches.Where(w => w.CreationDate.Date == DateTime.UtcNow.Date && w.Status == ConsolidationStatusEnum.Launched).Select(l => new LaunchDTO
        {
            Id = l.Id,
            Amount = l.Amount,
            LaunchType = l.LaunchType,
            CreationDate = l.CreationDate,
            ModificationDate = l.ModificationDate,
            Status = l.Status,
            ProductsOrder = l.LaunchProducts.Select(lp => new ProductsOrderDTO
            {
                ProductId = lp.ProductId,
                Quantity = lp.ProductQuantity,
                Price = lp.ProductPrice
            }).ToList()
        }).ToList();
    }

    public async Task<ApiResponse> UpdateLaunchConsolidation(List<LaunchDTO> launches)
    {
        try
        {
            foreach (var launch in launches)
            {
                LaunchEntity launchEntity = new LaunchEntity
                {
                    Id = launch.Id,
                    Amount = launch.Amount,
                    LaunchType = launch.LaunchType,
                    CreationDate = launch.CreationDate,
                    Status = launch.Status,
                    ModificationDate = launch.ModificationDate
                };

                await _repository.UpdateAsync(launchEntity);
            }

            return new ApiResponse { Success = true, Message = "Launch updated successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the launch.");
            return new ApiResponse { Success = false, Message = "An error occurred while updating the launch." };
        }
    }

    #region private methods
    private async Task<ApiResponse> GenerateLaunchProduct(List<ProductsOrder> productsOrder, LaunchEntity launch)
    {
        foreach (var productOrder in productsOrder)
        {
            var product = await _productRepository.GetByIdAsync(productOrder.ProductId);

            if (product == null)
            {
                _logger.LogWarning($"Product with id {productOrder.ProductId} not found.");
                return new ApiResponse { Message = $"Product with id {productOrder.ProductId} not found.", Success = false };
            }

            if (product.Stock < productOrder.Quantity)
            {
                _logger.LogWarning($"There is no stock of {product.Name} to launch; Stock: {product.Stock}, Quantity: {productOrder.Quantity}");
                return new ApiResponse { Message = $"There is no stock of {product.Name} to launch; Stock: {product.Stock}, Quantity: {productOrder.Quantity}", Success = false };
            }

            _context.Entry(product).State = EntityState.Detached;

            await _launchProductRepository.AddAsync(new LaunchProductEntity
            {
                LaunchId = launch.Id,
                ProductId = product.Id,
                ProductQuantity = productOrder.Quantity,
                ProductPrice = product.Price
            });

            await RemoveProductsFromStock(product, productOrder.Quantity);
        }

        await _unitOfWork.CommitAsync();

        return new ApiResponse { Success = true, Message = "Launch created successfully." };
    }

    private async Task RemoveProductsFromStock(ProductEntity product, int productOrderQuantity)
    {
        product.Stock = product.Stock - productOrderQuantity;

        await _productRepository.UpdateAsync(product);
    }

    private async Task<LaunchEntity> GenerateLaunch(LaunchTypeEnum launchType, List<ProductsOrder> productsOrder)
    {
        if (productsOrder.Count <= 0)
        {
            _logger.LogError("Products must be inserted.");
            throw new ArgumentException("Products must be inserted.");
        }

        var launch = new LaunchEntity()
        {
            Amount = await CalculateAmountAsync(productsOrder),
            LaunchType = launchType,
            Status = ConsolidationStatusEnum.Launched,
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };

        await _repository.AddAsync(launch);
        await _unitOfWork.CommitAsync();

        if (launch.Id <= 0)
        {
            _logger.LogWarning("Launch ID was not generated.");
            throw new InvalidOperationException("Launch ID was not generated.");
        }

        return launch;
    }

    private async Task<decimal> CalculateAmountAsync(List<ProductsOrder> productsOrder)
    {
        decimal totalAmount = 0m;

        foreach (var productOrder in productsOrder)
        {
            var product = await _productRepository.FindAsync(p => p.Id == productOrder.ProductId);

            if (product == null)
            {
                throw new Exception($"Product with id {productOrder.ProductId} not found.");
            }

            totalAmount += product.First().Price * productOrder.Quantity;
        }

        return totalAmount;
    }
    #endregion
}