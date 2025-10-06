using CashFlow.Application.Commands;
using CashFlow.Application.CoR;
using CashFlow.UnitTest.Application.Commands;
using FluentValidation;
using MediatR;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.UnitTest.Application.CoR;

public class ValidateCommandCoRTests
{
    private readonly Mock<RequestHandlerDelegate<CommandResponse<Guid>>> _delegate =
        new();

    private readonly ValidateCommandCoR<AddTransactionDailyCommand, Guid> _validateCommandChain;

    private readonly IEnumerable<IValidator<AddTransactionDailyCommand>> _validators =
        new List<IValidator<AddTransactionDailyCommand>>
        {
            new AddTransactionCommandValidator()
        };

    public ValidateCommandCoRTests()
    {
        _validateCommandChain = new ValidateCommandCoR<AddTransactionDailyCommand, Guid>(_validators);
    }

    [Theory]
    [ClassData(typeof(AddTransactionCommandFailTestData))]
    public async Task ValidateCommand_ReturnError(AddTransactionDailyCommand dailyCommand)
    {
        // Act
        var (isSuccess, data, error) =
            await _validateCommandChain.Handle(dailyCommand, _delegate.Object, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.NotNull(error);
        Assert.Equal(CommandInvalid, error!.ErrorCode);
        Assert.Equal(default, data);
    }

    [Theory]
    [ClassData(typeof(AddTransactionCommandTestData))]
    public async Task ValidateCommand_ReturnSuccess(AddTransactionDailyCommand dailyCommand)
    {
        // Arrange
        _delegate.Setup(x => x.Invoke()).ReturnsAsync(CommandResponse<Guid>.CreateSuccess(Guid.NewGuid()));

        // Act
        var (isSuccess, data, error) =
            await _validateCommandChain.Handle(dailyCommand, _delegate.Object, CancellationToken.None);

        // Assert
        Assert.True(isSuccess);
        Assert.Null(error);
        Assert.NotEqual(default, data);
    }
}