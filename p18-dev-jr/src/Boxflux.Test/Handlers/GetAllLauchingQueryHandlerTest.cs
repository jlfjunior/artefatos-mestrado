using Boxflux.Application.Commands.Lauchings;
using Boxflux.Application.Handlers.Lauchings;
using Boxflux.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Boxflux.Test;

public class GetAllLauchingQueryHandlerTest
{
    [Fact]
    public async Task Handle_ShouldReturnAllLauchings()
    {
        // Arrange
        var lancamentoRepositoryMock = new Mock<IGeralRepository<Lauching>>();
        var handler = new LauchingCommandHandler(lancamentoRepositoryMock.Object);
        var query = new LauchingCommand.GetAllLauchingQuery();
        var lancamentos = new List<Lauching>
        {
            new Lauching { Id = Guid.NewGuid(), Type = "crédito", Value = 100.0, DateLauching = DateTime.UtcNow },
            new Lauching { Id = Guid.NewGuid(), Type = "débito", Value = 50.0, DateLauching = DateTime.UtcNow }
        };

        lancamentoRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(lancamentos);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(lancamentos);
        lancamentoRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }
}
