using EasyCash.Command.Users.Login;
using EasyCash.Command.Users.Register;
using EasyCash.Integration.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EasyCash.Integration.Tests.Users;

public class LoginUserTests : BaseFunctionalTest
{
    private const string Email = "login@test.com";
    private const string Password = "12345";

    public LoginUserTests(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LogInUserCommand(Email, Password);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/v1/users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var registerRequest = new RegisterUserCommand(Email, "first", "last", Password);
        await HttpClient.PostAsJsonAsync("api/v1/users/register", registerRequest);

        var request = new LogInUserCommand(Email, Password);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/v1/users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
