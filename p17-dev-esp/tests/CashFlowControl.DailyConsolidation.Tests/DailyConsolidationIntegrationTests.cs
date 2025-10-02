using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Application.Services;
using CashFlowControl.Core.Domain.Entities;
using CashFlowControl.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http.Json;

namespace CashFlowControl.DailyConsolidation.Tests
{
    public class DailyConsolidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<ITransactionHttpClientService> _mockTransactionHttpClientService;
        private readonly Mock<IConsolidatedBalanceRepository> _mockBalanceRepo;
        private readonly Mock<ILogger<DailyConsolidationService>> _mockLogger;
        private readonly Mock<IMediator> _mediator;

        private readonly HttpClient _client;
        private readonly IDailyConsolidationService _dailyConsolidationService;

        public DailyConsolidationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _mockTransactionHttpClientService = new Mock<ITransactionHttpClientService>();
            _mockBalanceRepo = new Mock<IConsolidatedBalanceRepository>();
            _mockLogger = new Mock<ILogger<DailyConsolidationService>>();
            _mediator = new Mock<IMediator>(); 

            var webAppFactory = new WebApplicationFactory<Program>(); 
            _client = webAppFactory.CreateClient();

            _dailyConsolidationService = new DailyConsolidationService(_mockTransactionHttpClientService.Object, _mockBalanceRepo.Object, _mockLogger.Object, _mediator.Object);
        }

        [Fact]
        public async Task ProcessTransactionAndConsolidateBalance_ShouldWork()
        {
            // Arrange
            var transaction = new CreateTransactionDTO
            {
                Amount = 100,
                Type = TransactionType.Credit.ToString()
            };

            // Act - Enviar transação para a API
            var response = await _client.PostAsJsonAsync("/api/Transaction", transaction);
            response.EnsureSuccessStatusCode();

            var transactionUrl = response.Headers.Location?.ToString();
            Assert.NotNull(transactionUrl); 

            // Act - Consultar a API com a URL retornada
            var transactionResponse = await _client.GetAsync(transactionUrl);
            transactionResponse.EnsureSuccessStatusCode();

            // Assert - Validar que a transação retornada é a mesma que foi criada
            var transactionCreated = await transactionResponse.Content.ReadFromJsonAsync<TransactionCreatedDTO>();
            Assert.NotNull(transactionCreated);
            Assert.Equal(transaction.Amount, transactionCreated.Amount);
            Assert.Equal(transaction.Type, transactionCreated.Type);

            var mappedTransaction = new CreateTransactionDTO
            {
                Amount = transactionCreated.Amount,
                CreatedAt = transactionCreated.CreatedAt,
                Id = transactionCreated.Id,
                Type = transactionCreated.Type
            };

            var expectedTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Amount = mappedTransaction.Amount,
                    CreatedAt = mappedTransaction.CreatedAt,
                    Id = mappedTransaction.Id,
                    Type = mappedTransaction.Type 
                }
            };

            _mockTransactionHttpClientService
                .Setup(service => service.GetTransactionsByDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(expectedTransactions);

            // Act - Consolidar o saldo no serviço
            await _dailyConsolidationService.ProcessTransactionAsync(mappedTransaction);

            // Assert - Verificar se o saldo foi consolidado corretamente
            var consolidatedBalance = await _dailyConsolidationService.GetConsolidatedBalanceByDateAsync(mappedTransaction.CreatedAt.Date);
            Assert.NotNull(consolidatedBalance.Value);
            Assert.Equal(100, consolidatedBalance.Value.ConsolidatedBalance.TotalCredit);
        }
    }

}
