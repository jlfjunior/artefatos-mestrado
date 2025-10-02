using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.ValueObjets;
using EasyCash.Unit.Tests.Infrastructure;
using FluentAssertions;

namespace EasyCash.Unit.Tests.Domain.Users;

public class UserEntityTests : BaseTest
{
    [Fact]
    public void Create_Should_SetPropertyValue()
    {
        // Act
        var user = UserEntity.Create(UserData.FirstName, UserData.LastName, UserData.Email);

        // Assert
        user.FirstName.Should().Be(UserData.FirstName);
        user.LastName.Should().Be(UserData.LastName);
        user.Email.Should().Be(UserData.Email);
    }

    [Fact]
    public void Create_ShouldInitializeUserEntity()
    {
        // Arrange
        var firstName = new FirstName("John");
        var lastName = new LastName("Doe");
        var email = Email.Instance("john.doe@example.com");

        // Act
        var user = UserEntity.Create(firstName, lastName, email);

        // Assert
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
    }

    [Fact]
    public void SetIdentityId_ShouldUpdateIdentityId()
    {
        // Arrange
        var user = UserEntity.Create(new FirstName("John"), new LastName("Doe"), Email.Instance("john.doe@example.com"));
        var identityId = "new-identity-id";

        // Act
        user.SetIdentityId(identityId);

        // Assert
        user.IdentityId.Should().Be(identityId);
    }

    [Fact]
    public void InitialTestUser_ShouldBeInitializedCorrectly()
    {
        // Act
        var user = UserEntity.InitialTestUser;

        // Assert
        user.FirstName.Should().Be(new FirstName("Test"));
        user.LastName.Should().Be(new LastName("User"));
        user.Email.Should().Be(Email.Instance("test@easycash.com.br"));
    }
}
