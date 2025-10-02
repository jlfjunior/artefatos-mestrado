using System;
using System.Collections.Generic;
using Application.Launch.Product.Query.GetAllProducts;
using Application.Launch.Product.Query.GetPaginatedProducts;
using Application.Launch.Product.Query.GetProductById;
using Domain.DTOs;
using Domain.DTOs.Launch;
using Domain.Models;
using Domain.Models.Launch.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ProductControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<ProductController>> _loggerMock;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Create_ReturnsOk_WhenProductIsValid()
    {
        var product = new ProductModel { Name = "Test Product", Price = 10.0m, Stock = 5 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ApiResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _controller.Create(product);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenProductIsNull()
    {
        var result = await _controller.Create(null!);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithListOfProducts()
    {
        var products = new List<ProductModel> { new ProductModel { Name = "Product1", Price = 10, Stock = 5 } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ApiResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUpdateIsSuccessful()
    {
        var product = new ProductModel { Name = "Updated Product", Price = 15, Stock = 10 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ApiResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _controller.Update(1, product);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenProductIsNull()
    {
        var result = await _controller.Update(1, null!);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenDeleteIsSuccessful()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ApiResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenIdIsNull()
    {
        var result = await _controller.Delete(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetByName_ShouldReturnOkResult_WhenProductsAreFound()
    {
        var mockResult = new List<ProductDTO>
        {
            new ProductDTO { Id = 1, Name = "Test Product", Price = 100 },
            new ProductDTO { Id = 2, Name = "Another Product", Price = 150 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllProductsQuery>(), default))
            .ReturnsAsync(mockResult);

        string name = "Test";

        var result = await _controller.GetByName(name);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPaginated_ShouldReturnOkResult_WhenDataIsAvailable()
    {
        var mockResult = new PaginatedResult<ProductDTO>(
            new List<ProductDTO>
            {
                new ProductDTO { Id = 1, Name = "Test Product", Price = 100 }
            },
            100,
            10,
            1
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPaginatedProductsQuery>(), default))
            .ReturnsAsync(mockResult);

        int pageNumber = 1;
        int pageSize = 10;

        var result = await _controller.GetPaginated(pageNumber, pageSize);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkResult_WhenProductIsFound()
    {
        var mockResult = new ProductDTO { Id = 1, Name = "Test Product", Price = 100 };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductsByIdQuery>(), default))
            .ReturnsAsync(mockResult);

        int id = 1;

        var result = await _controller.GetById(id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductIsNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductsByIdQuery>(), default))
            .ReturnsAsync((ProductDTO)null!);

        int id = 1;

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
