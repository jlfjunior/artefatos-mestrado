using Consolidation.Application.UseCases.ProcessEntryCreated;
using Consolidation.Domain.DTOs;
using Consolidation.Domain.Entities;
using Consolidation.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Consolidation.Tests.Application.UseCases
{
    public sealed class ProcessEntryCreatedHandlerTests
    {
        private readonly Mock<IDailyBalanceRepository> _repositoryMock = new();
        private readonly ProcessEntryCreatedHandler _handler;

        public ProcessEntryCreatedHandlerTests()
        {
            _handler = new ProcessEntryCreatedHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task HandleAsync_CreditEntry_NewDate_ShouldCreateAndApplyCredit()
        {
            var message = new EntryCreatedMessage(
                EventId: Guid.NewGuid(),
                EntryId: Guid.NewGuid(),
                Amount: 100,
                Currency: "BRL",
                Type: 2, // Credit
                Description: "Test credit",
                Date: DateTime.Today,
                OccurredOn: DateTime.UtcNow);

            _repositoryMock
                .Setup(r => r.GetByDateAsync(message.Date, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DailyBalance?)null);

            await _handler.HandleAsync(message);

            _repositoryMock.Verify(r => r.AddAsync(
                It.Is<DailyBalance>(d => d.TotalCredits == 100),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_DebitEntry_NewDate_ShouldCreateAndApplyDebit()
        {
            var message = new EntryCreatedMessage(
                EventId: Guid.NewGuid(),
                EntryId: Guid.NewGuid(),
                Amount: 50,
                Currency: "BRL",
                Type: 1, // Debit
                Description: "Test debit",
                Date: DateTime.Today,
                OccurredOn: DateTime.UtcNow);

            _repositoryMock
                .Setup(r => r.GetByDateAsync(message.Date, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DailyBalance?)null);

            await _handler.HandleAsync(message);

            _repositoryMock.Verify(r => r.AddAsync(
                It.Is<DailyBalance>(d => d.TotalDebits == 50),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_CreditEntry_ExistingDate_ShouldUpdateBalance()
        {
            var existingBalance = DailyBalance.Create(DateTime.Today, "BRL");
            existingBalance.ApplyCredit(100);

            var message = new EntryCreatedMessage(
                EventId: Guid.NewGuid(),
                EntryId: Guid.NewGuid(),
                Amount: 50,
                Currency: "BRL",
                Type: 2, // Credit
                Description: "Test credit",
                Date: DateTime.Today,
                OccurredOn: DateTime.UtcNow);

            _repositoryMock
                .Setup(r => r.GetByDateAsync(message.Date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBalance);

            await _handler.HandleAsync(message);

            _repositoryMock.Verify(r => r.UpdateAsync(
                It.Is<DailyBalance>(d => d.TotalCredits == 150),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ExistingDate_ShouldNotCallAddAsync()
        {
            var existingBalance = DailyBalance.Create(DateTime.Today, "BRL");

            var message = new EntryCreatedMessage(
                EventId: Guid.NewGuid(),
                EntryId: Guid.NewGuid(),
                Amount: 100,
                Currency: "BRL",
                Type: 2,
                Description: "Test",
                Date: DateTime.Today,
                OccurredOn: DateTime.UtcNow);

            _repositoryMock
                .Setup(r => r.GetByDateAsync(message.Date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBalance);

            await _handler.HandleAsync(message);

            _repositoryMock.Verify(r => r.AddAsync(
                It.IsAny<DailyBalance>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
