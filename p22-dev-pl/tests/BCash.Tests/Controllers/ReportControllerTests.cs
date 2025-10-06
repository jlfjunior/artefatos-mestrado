using AutoMapper;
using BCash.Domain.Entities;
using BCash.Domain.Services;
using BCash.ReportApi.Controllers;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BCash.Tests.Controllers
{
    public class ReportControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ITransactionService> mockTransactionService;
        private Mock<IBalanceService> mockBalanceService;
        private Mock<IMapper> mockMapper;

        public ReportControllerTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Default);

            this.mockTransactionService = this.mockRepository.Create<ITransactionService>();
            this.mockBalanceService = this.mockRepository.Create<IBalanceService>();
            this.mockMapper = this.mockRepository.Create<IMapper>();
        }

        private ReportController CreateReportController()
        {
            return new ReportController(
                this.mockTransactionService.Object,
                this.mockBalanceService.Object,
                this.mockMapper.Object);
        }

        [Fact]
        public async Task GetBalanceWithOffsetPagination_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var reportController = this.CreateReportController();
            DateTime initDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await reportController.GetBalanceWithOffsetPagination(
                initDate,
                endDate,
                pageNumber,
                pageSize);

            // Assert
            Assert.True(true);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetTransactionsWithOffsetPagination_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var reportController = this.CreateReportController();
            DateTime? initDate = DateTime.Now;
            DateTime? endDate = DateTime.Now;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await reportController.GetTransactionsWithOffsetPagination(
                initDate,
                endDate,
                pageNumber,
                pageSize);

            // Assert
            Assert.True(true);
            this.mockRepository.VerifyAll();
        }
    }
}
