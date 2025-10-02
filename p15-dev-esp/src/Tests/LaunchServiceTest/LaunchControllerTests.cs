using Application.Launch.Launch.Command.CreateLaunch;
using Application.Launch.Launch.Query.GetAllLaunches;
using Domain.DTOs.Launch;
using Domain.Models;
using Domain.Models.Launch;
using Domain.Models.Launch.Launch;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LaunchService.Tests
{
    public class LaunchControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<LaunchController>> _loggerMock;
        private readonly LaunchController _controller;

        public LaunchControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<LaunchController>>();
            _controller = new LaunchController(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenLaunchIsNull()
        {
            // Arrange
            LaunchModel launch = null!;

            // Act
            var result = await _controller.Create(launch);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid launch request.", badRequestResult.Value);
        }

        [Fact]
        public async Task Create_ShouldReturnOkResult_WhenLaunchIsValid()
        {
            // Arrange
            var launch = new LaunchModel
            {
                LaunchType = LaunchTypeEnum.Debit,
                ProductsOrder = new List<ProductsOrder>()
                {
                    new ProductsOrder { ProductId = 1, Quantity = 2 }
                }
            };

            var mockResult = new ApiResponse() { Success = true };

            _mediatorMock.Setup(m => m.Send(It.IsAny<ApiResponse>(), default))
                .ReturnsAsync(mockResult);

            var result = await _controller.Create(launch);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResult_WhenDataIsAvailable()
        {
            // Arrange
            var mockResult = new List<LaunchDTO>
            {
                new LaunchDTO
                {
                    Id = 1,
                    Amount = 100,
                    CreationDate = DateTime.UtcNow,
                    LaunchType = LaunchTypeEnum.Credit,
                    Status = ConsolidationStatusEnum.Launched,
                    ModificationDate = DateTime.UtcNow,
                    ProductsOrder = new List<ProductsOrderDTO>
                    {
                        new ProductsOrderDTO { ProductId = 1, Quantity = 2 }
                    }
                }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllLaunchesQuery>(), default))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<LaunchDTO>>(okResult.Value);
            Assert.Equal(mockResult.Count, returnValue.Count);
            Assert.Equal(mockResult[0].Id, returnValue[0].Id);
        }

        [Fact]
        public async Task GetAll_ShouldReturnNoContent_WhenNoDataIsAvailable()
        {
            // Arrange
            var mockResult = new List<LaunchDTO>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllLaunchesQuery>(), default))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<LaunchDTO>>(okResult.Value);
            Assert.Empty(returnValue);
        }
    }
}
