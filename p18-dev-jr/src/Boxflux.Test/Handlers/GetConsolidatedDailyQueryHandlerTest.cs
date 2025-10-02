using Boxflux.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Boxflux.Test;

public class GetConsolidatedDailyQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnConsolidatedBalance()
    {
        var random = new Random();

        for (int i = 0; i < 50; i++)
        {
            
            var consolidatedRepositoryMock = new Mock<IGeralRepository<ConsolidatedDaily>>();
            var lancamentoRepositoryMock = new Mock<IGeralRepository<Lauching>>();
            var handler = new ConsolidatedDailyCommandHandler(consolidatedRepositoryMock.Object, lancamentoRepositoryMock.Object);
            var query = new ConsolidatedDailyCommand.GetConsolidatedDailyQuery
            {
                Date = DateTime.UtcNow.AddDays(-i)
            };

            var lancamentos = new List<Lauching>
            {
                new Lauching { Id = Guid.NewGuid(), Type = "crédito", Value = random.Next(150, 15000), DateLauching = DateTime.UtcNow.AddMinutes(-random.Next(0, 60)) },
                new Lauching { Id = Guid.NewGuid(), Type = "débito", Value = random.Next(10, 7000), DateLauching = DateTime.UtcNow.AddMinutes(-random.Next(0, 60)) }
            };

            lancamentoRepositoryMock.Setup(repo => repo.GetAllByDateAsync(query.Date)).ReturnsAsync(lancamentos);

            
            var result = await handler.Handle(query, CancellationToken.None);

           
            result.Should().Be(lancamentos.Where(x => x.Type == "crédito").Sum(x => x.Value) - lancamentos.Where(x => x.Type == "débito").Sum(x => x.Value));
            consolidatedRepositoryMock.Verify(repo => repo.AddOrUpdateAsync(It.IsAny<Guid>(), It.IsAny<ConsolidatedDaily>()), Times.Once);
        }
    }
}

