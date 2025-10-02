using Boxflux.Application.Commands.Lauchings;
using Boxflux.Application.Handlers.Lauchings;
using Boxflux.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Boxflux.Test;
public class ConsolidatedDailyCommandHandlerTest
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnTrue()
    {
        // Arrange
        var lancamentoRepositoryMock = new Mock<IGeralRepository<Lauching>>();
        var handler = new LauchingCommandHandler(lancamentoRepositoryMock.Object);
        var command = new LauchingCommand.CreateLauchingCommand
        {
            Type = "crédito",
            Value = 100.0,
            DateLauching = DateTime.UtcNow
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        lancamentoRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Lauching>()), Times.Once);
    }
}