using Application.Consolidation.Consolidation.Query.GetAllConsolidations;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ConsolidationService.Controllers;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.DTOs.Consolidation;
using Application.Product.Product.Query.GetPaginatedConsolidations;
using Domain.DTOs;

namespace ConsolidationService.Tests
{
    public class ConsolidationControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<ConsolidationController>> _loggerMock;
        private readonly ConsolidationController _controller;

        public ConsolidationControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<ConsolidationController>>();
            _controller = new ConsolidationController(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResult_WhenDataIsAvailable()
        {
            // Arrange
            var mockResult = new List<ConsolidationDTO>
            {
                new ConsolidationDTO {
                    Id = new Guid().ToString(),
                    Date = DateTime.UtcNow,
                    TotalAmount = 100,
                    TotalCreditAmount = 50,
                    TotalDebitAmount = 40
                }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllConsolidationsQuery>(), default))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<ConsolidationDTO>>(okResult.Value);
            Assert.Equal(mockResult, returnValue);
        }

        [Fact]
        public async Task GetAll_ShouldReturnNoContent_WhenNoDataIsAvailable()
        {
            // Arrange
            var mockResult = new List<ConsolidationDTO>();
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllConsolidationsQuery>(), default))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<List<ConsolidationDTO>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetPaginated_ShouldReturnOkResult_WhenDataIsAvailable()
        {
            // Arrange
            var mockResult = new PaginatedResult<ConsolidationDTO>(
                new List<ConsolidationDTO>
                {
                    new ConsolidationDTO
                    {
                        Id = new Guid().ToString(),
                        Date = DateTime.UtcNow,
                        TotalAmount = 100,
                        TotalCreditAmount = 50,
                        TotalDebitAmount = 40
                    }
                },
                100,
                10,
                1
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPaginatedConsolidationsQuery>(), default))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.GetPaginated(1, 10);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
