using Moq;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using CashFlowService.API.Controllers;

namespace CashFlowService.Tests
{
    public class CashFlowControllerTests
    {
        [Fact]
        public async Task CreateTransaction_ReturnsOkResult_WhenValidRequest()
        {
            // Arrange
            var cashFlowServiceMock = new Mock<ICashFlowService>();
            cashFlowServiceMock.Setup(s => s.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>()))
                .ReturnsAsync(new TransactionResponse { TransactionId = 1, Date = "2025-02-18", Amount = 100, Type = "Credit", Description = "Test" });

            var controller = new CashFlowController(cashFlowServiceMock.Object);
            var request = new CreateTransactionRequest { Date = "2025-02-18", Amount = 100, Type = "Credit", Description = "Test" };

            // Act
            var result = await controller.CreateTransaction(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}