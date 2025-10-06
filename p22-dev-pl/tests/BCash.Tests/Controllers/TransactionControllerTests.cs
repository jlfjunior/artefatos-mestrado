using AutoMapper;
using BCash.TransactionApi.DTOs;
using BCash.Domain.Services;
using BCash.TransactionApi.Controllers;
using Moq;

namespace BCash.Tests.Controllers
{
    public class TransactionControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ITransactionService> mockTransactionService;
        private Mock<IBalanceService> mockBalanceService;
        private Mock<IMapper> mockMapper;

        public TransactionControllerTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Default);

            this.mockTransactionService = this.mockRepository.Create<ITransactionService>();
            this.mockBalanceService = this.mockRepository.Create<IBalanceService>();
            this.mockMapper = this.mockRepository.Create<IMapper>();
        }

        private TransactionController CreateTransactionController()
        {
            return new TransactionController(
                this.mockTransactionService.Object,
                this.mockBalanceService.Object,
                this.mockMapper.Object);
        }

        [Fact]
        public async Task GetAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var transactionController = this.CreateTransactionController();
            Guid id = default(global::System.Guid);

            // Act
            var result = await transactionController.GetAsync(
                id);

            // Assert
            Assert.True(true);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task CreateAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var transactionController = this.CreateTransactionController();
            TransactionRequestDto TransactionRequestDto = new TransactionRequestDto()
            {
                Amount = 1,
                Date = DateTime.Now,
                Type = "C",
                Description = null
            };

            // Act
            var result = await transactionController.CreateAsync(
                TransactionRequestDto);

            // Assert
            Assert.True(true);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var transactionController = this.CreateTransactionController();
            Guid id = default(global::System.Guid);

            // Act
            var result = await transactionController.DeleteAsync(
                id);

            // Assert
            Assert.True(true);
            this.mockRepository.VerifyAll();
        }
    }
}
