using EasyCash.Domain.Users.ValueObjets;
using FluentAssertions;

namespace EasyCash.Unit.Tests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Email_Should_ConvertToLowerCase()
    {
        // Arrange
        var email = "Test@Example.com";

        // Act
        var emailObj = new Email(email);

        // Assert
        emailObj.Mail.Should().Be("test@example.com");
    }

    [Fact]
    public void Email_Should_ReturnEmptyString_When_NullOrEmpty()
    {
        // Arrange
        var email = "";

        // Act
        var emailObj = new Email(email);

        // Assert
        emailObj.Mail.Should().BeEmpty();
    }

    [Fact]
    public void Email_Instance_Should_ReturnEmailObject()
    {
        // Arrange
        var email = "Test@Example.com";

        // Act
        var emailObj = Email.Instance(email);

        // Assert
        emailObj.Should().NotBeNull();
        emailObj.Mail.Should().Be("test@example.com");
    }

    [Fact]
    public void Email_Should_BeValueObject()
    {
        // Arrange
        var email1 = new Email("Test@Example.com");
        var email2 = new Email("Test@Example.com");

        // Act & Assert
        email1.Should().Be(email2);
    }
}