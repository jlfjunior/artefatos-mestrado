using EasyCash.Command.Users.Register;
using EasyCash.Integration.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EasyCash.Integration.Tests.Users;

public class RegisterUserTests : BaseFunctionalTest
{
    public RegisterUserTests(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Theory]
    [InlineData("", "first", "last", "12345")]
    [InlineData("test.com", "first", "last", "12345")]
    [InlineData("@test.com", "first", "last", "12345")]
    [InlineData("test@", "first", "last", "12345")]
    [InlineData("test@test.com", "", "last", "12345")]
    [InlineData("test@test.com", "first", "", "12345")]
    [InlineData("test@test.com", "first", "last", "")]
    [InlineData("test@test.com", "first", "last", "1")]
    [InlineData("test@test.com", "first", "last", "12")]
    [InlineData("test@test.com", "first", "last", "123")]
    [InlineData("test@test.com", "first", "last", "1234")]
    public async Task Register_ShouldReturnBadRequest_WhenRequestIsInvalid(
        string email,
        string firstName,
        string lastName,
        string password)
    {
        // Arrange
        var request = new RegisterUserCommand(email, firstName, lastName, password);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new RegisterUserCommand("create@test.com", "first", "last", "12345");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
