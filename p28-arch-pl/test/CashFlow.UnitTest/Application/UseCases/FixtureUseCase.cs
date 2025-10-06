using Microsoft.Extensions.Logging;

namespace CashFlow.UnitTest.Application.UseCases;

public class FixtureUseCase<TUseCase>
{
    protected readonly Mock<ILogger<TUseCase>> _logger = new();

    protected void LoggerVerify(LogLevel logLevel, string logMessage)
    {
        _logger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(logMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}