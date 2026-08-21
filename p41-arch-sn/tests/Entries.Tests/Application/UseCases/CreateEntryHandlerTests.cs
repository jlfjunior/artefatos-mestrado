using Entries.Application.DTOs;
using Entries.Application.UseCases.CreateEntry;
using Entries.Domain.Enums;
using Entries.Domain.Interfaces;
using Entries.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Entries.Tests.Application.UseCases
{
    public sealed class CreateEntryHandlerTests
    {
        private readonly Mock<IEntryRepository> _repositoryMock = new();
        private readonly Mock<IMessagePublisher> _publisherMock = new();
        private readonly CreateEntryHandler _handler;

        public CreateEntryHandlerTests()
        {
            _handler = new CreateEntryHandler(_repositoryMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ShouldReturnEntryResponse()
        {
            var request = new CreateEntryRequest(100, "BRL", EntryType.Credit, "Test entry", DateTime.Today);

            var response = await _handler.HandleAsync(request);

            response.Should().NotBeNull();
            response.Amount.Should().Be(100);
            response.Currency.Should().Be("BRL");
            response.Type.Should().Be(EntryType.Credit);
            response.Description.Should().Be("Test entry");
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ShouldCallRepositoryAddAsync()
        {
            var request = new CreateEntryRequest(100, "BRL", EntryType.Credit, "Test entry", DateTime.Today);

            await _handler.HandleAsync(request);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Entries.Domain.Entities.Entry>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ShouldPublishEntryCreatedEvent()
        {
            var request = new CreateEntryRequest(100, "BRL", EntryType.Credit, "Test entry", DateTime.Today);

            await _handler.HandleAsync(request);

            _publisherMock.Verify(p => p.PublishAsync(
                It.IsAny<Entries.Domain.Events.EntryCreatedEvent>(),
                "entry.created",
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_InvalidCurrency_ShouldThrowException()
        {
            var request = new CreateEntryRequest(100, "XYZ", EntryType.Credit, "Test entry", DateTime.Today);

            var act = async () => await _handler.HandleAsync(request);

            await act.Should().ThrowAsync<Entries.Domain.Exceptions.InvalidCurrencyException>();
        }

        [Fact]
        public async Task HandleAsync_ZeroAmount_ShouldThrowException()
        {
            var request = new CreateEntryRequest(0, "BRL", EntryType.Credit, "Test entry", DateTime.Today);

            var act = async () => await _handler.HandleAsync(request);

            await act.Should().ThrowAsync<Entries.Domain.Exceptions.InvalidAmountException>();
        }
    }
}
