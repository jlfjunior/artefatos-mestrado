using Application.Launch.Product.Command.CreateProduct;
using Application.Launch.Product.Command.DeleteProduct;
using Application.Launch.Product.Command.UpdateProduct;
using Application.Launch.Product.Query.GetAllProducts;
using Application.Launch.Product.Query.GetPaginatedProducts;
using Application.Launch.Product.Query.GetProductById;
using Domain.Models.Launch.Product;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IMediator mediator, ILogger<ProductController> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductModel product)
    {
        try
        {
            if (product == null)
            {
                _logger.LogError("Invalid product request.");
                return BadRequest("Invalid product request.");
            }

            _logger.LogDebug($"Product creating attempt of {product.Name}");

            var productCommand = new CreateProductCommand() { Name = product.Name, Price = product.Price, Stock = product.Stock };

            var response = await _mediator.Send(productCommand);
            _logger.LogDebug($"Product {productCommand.Name} created successfully.");
            return Ok(response);
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to create product {product.Name}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAllProductsQuery());

            _logger.LogInformation("Retrieved all products.");
            return Ok(result);
        }
        catch(Exception)
        {
            _logger.LogError("Failed to retrieve products.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("/api/Product/ByName/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        try
        {
            var result = await _mediator.Send(new GetAllProductsQuery());

            var filteredProducts = result.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();

            _logger.LogInformation($"Retrieved {filteredProducts.Count} products matching '{name}'.");

            return Ok(filteredProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to retrieve products. Error: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("/api/Product/Paginated")]
    public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var result = await _mediator.Send(new GetPaginatedProductsQuery { PageNumber = pageNumber, PageSize = pageSize });

            _logger.LogInformation("Retrieved paginated products.");

            return Ok(new
            {
                Total = result.TotalCount,
                Products = result.Items,
                result.PageSize,
                result.PageNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve products.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("/api/Product/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _mediator.Send(new GetProductsByIdQuery { Id = id });

            if (result == null)
            {
                _logger.LogWarning($"Product with ID {id} not found.");
                return NotFound(new { Message = "Product not found." });
            }

            _logger.LogInformation($"Retrieved product {result.Name}.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve product.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }



    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductModel product)
    {
        try
        {
            if (product == null)
            {
                _logger.LogError("Invalid product request.");
                return BadRequest("Invalid product request.");
            }

            _logger.LogDebug($"Product updating attempt of {product.Name}");

            var updateCommand = new UpdateProductCommand() { Id = id, Name = product.Name, Price = product.Price, Stock = product.Stock };

            var response = await _mediator.Send(updateCommand);
            _logger.LogDebug($"Product {updateCommand.Name} updated successfully.");
            return Ok(response);
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to update product {product.Name}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        try
        {
            _logger.LogDebug($"Product deleting attempt of product id {id}");

            if (id == null)
            {
                _logger.LogError("Invalid product request.");
                return BadRequest("Invalid product request.");
            }
            var deleteCommand = new DeleteProductCommand() { Id = id.Value };

            var response = await _mediator.Send(deleteCommand);
            _logger.LogDebug($"Product {deleteCommand.Id} created successfully.");
            return Ok(response);
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to delete product {id}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
