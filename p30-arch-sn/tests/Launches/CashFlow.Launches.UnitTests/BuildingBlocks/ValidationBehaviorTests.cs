using CashFlow.BuildingBlocks.Behaviors;
using CashFlow.BuildingBlocks.Results;
using FluentAssertions;
using FluentValidation;

namespace CashFlow.Launches.UnitTests.BuildingBlocks;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNext_WhenThereAreNoValidators()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result<string>>(Array.Empty<IValidator<TestRequest>>());

        // Act
        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ => Task.FromResult(Result<string>.Success("next-called")),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("next-called");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenRequestIsValid()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result<string>>([new TestRequestValidator()]);

        // Act
        var result = await behavior.Handle(
            new TestRequest("valid"),
            _ => Task.FromResult(Result<string>.Success("next-called")),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("next-called");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailureWithDetails_WhenRequestIsInvalidForResult()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>([new TestRequestValidator()]);

        // Act
        var result = await behavior.Handle(
            new TestRequest(string.Empty),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("validation.failed");
        result.Error.Message.Should().Be("One or more validation errors occurred.");
        result.Error.Details.Should().NotBeNull();
        result.Error.Details!.Should().ContainKey(nameof(TestRequest.Value));
        result.Error.Details[nameof(TestRequest.Value)].Should().ContainSingle()
            .Which.Should().Be("Value is required.");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailureWithDetails_WhenRequestIsInvalidForResultOfT()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result<string>>([new TestRequestValidator()]);

        // Act
        var result = await behavior.Handle(
            new TestRequest(string.Empty),
            _ => Task.FromResult(Result<string>.Success("next-called")),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("validation.failed");
        result.Error.Details.Should().NotBeNull();
        result.Error.Details!.Should().ContainKey(nameof(TestRequest.Value));
    }

    private sealed record TestRequest(string Value);

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required.");
        }
    }
}
