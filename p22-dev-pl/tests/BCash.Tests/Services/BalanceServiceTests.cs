using BCash.Domain.Repositories;
using BCash.Service.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BCash.Tests.Services
{
    public class BalanceServiceTests
    {
        private MockRepository mockRepository;

        private Mock<IBalanceRepository> mockBalanceRepository;

        private Mock<IDistributedCache> mockDistributedCache;

        public BalanceServiceTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Default);

            this.mockBalanceRepository = this.mockRepository.Create<IBalanceRepository>();

            this.mockDistributedCache = this.mockRepository.Create<IDistributedCache>();

        }

        private BalanceService CreateService()
        {
            return new BalanceService(
                this.mockBalanceRepository.Object,
                this.mockDistributedCache.Object);
        }

        [Fact]
        public async Task GetBalancePaged_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            DateTime initDate = default(global::System.DateTime);
            DateTime endDate = default(global::System.DateTime);
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await service.GetBalancePagedAsync(
                initDate,
                endDate,
                pageNumber,
                pageSize);

            // Assert
            Assert.True(true);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task ProcessBalance_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            decimal amount = 100;
            DateTime date = default(global::System.DateTime);
            string type = "C";

            // Act
            var result = await service.ProcessBalanceAsync(
                amount,
                date,
                type);

            // Assert
            Assert.True(true);
            this.mockRepository.VerifyAll();
        }
    }
}
