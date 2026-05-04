using CashFlow.Launches.Domain.Entities;
using CashFlow.Launches.Domain.Enums;
using CashFlow.Launches.Domain.Errors;
using FluentAssertions;

namespace CashFlow.Launches.UnitTests.Domain;

public sealed class LaunchTests
{
    [Fact]
    public void Create_ShouldSucceed_WhenDataIsValid()
    {
        // Arrange
        var occurredOnUtc = DateTime.UtcNow;

        // Act
        var result = Launch.Create(100m, LaunchType.Credit, occurredOnUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100m);
        result.Value.Type.Should().Be(LaunchType.Credit);
        result.Value.OccurredOnUtc.Should().Be(occurredOnUtc);
    }

    [Fact]
    public void Create_ShouldFail_WhenAmountIsInvalid()
    {
        // Arrange
        var occurredOnUtc = DateTime.UtcNow;

        // Act
        var result = Launch.Create(0m, LaunchType.Credit, occurredOnUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LaunchErrors.InvalidAmount);
        result.Error.Code.Should().Be("launch.invalid_amount");
        result.Error.Message.Should().Be("The launch amount must be greater than zero.");
    }

    [Fact]
    public void Create_ShouldFail_WhenTypeIsInvalid()
    {
        // Arrange
        var invalidType = (LaunchType)99;

        // Act
        var result = Launch.Create(10m, invalidType, DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LaunchErrors.InvalidType);
        result.Error.Code.Should().Be("launch.invalid_type");
        result.Error.Message.Should().Be("The launch type is invalid.");
    }
}
