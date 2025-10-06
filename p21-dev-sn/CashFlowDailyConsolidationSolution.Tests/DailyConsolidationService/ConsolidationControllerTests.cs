using Moq;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using DailyConsolidationService.API.Controllers;

namespace DailyConsolidationService.Tests
{
    public class ConsolidationControllerTests
    {
        [Fact]
        public async Task GetDailyReport_ReturnsOkResult_WhenReportExists()
        {
            // Arrange
            var consolidationServiceMock = new Mock<IConsolidationService>();
            consolidationServiceMock.Setup(s => s.GenerateDailyReportAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConsolidationReportResponse { Date = "2025-02-18", TotalCredits = 200, TotalDebits = 50, DailyBalance = 150 });

            var controller = new ConsolidationController(consolidationServiceMock.Object);

            // Act
            var result = await controller.GetDailyReport("2025-02-18");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}