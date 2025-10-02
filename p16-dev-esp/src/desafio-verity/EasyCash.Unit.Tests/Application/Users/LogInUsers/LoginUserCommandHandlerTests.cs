using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Exceptions;
using EasyCash.Command.Users.Login;
using EasyCash.Domain.Users.Errors;
using FluentAssertions;
using Moq;

namespace EasyCash.Unit.Tests.Application.Users.LogInUsers;

[Collection(nameof(UserTestsFixtureCollection))]
public class LoginUserCommandHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly Mock<ILoginService> _loginServiceMock;
    private readonly LogInUserCommandHandler _handler;

    public LoginUserCommandHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _loginServiceMock = new Mock<ILoginService>();
        _handler = new LogInUserCommandHandler(_loginServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAccessToken()
    {
        // Arrange
        var command = _fixture.LoginUserCommandFaker.Generate();
        var accessToken = "valid_token";

        _loginServiceMock
            .Setup(x => x.LoginAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be(accessToken);

        _loginServiceMock.Verify(x => x.LoginAsync(
            command.Email,
            command.Password,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var command = _fixture.LoginUserCommandFaker.Generate();

        _loginServiceMock
            .Setup(x => x.LoginAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .Throws(new BusinessException(UserErrors.InvalidCredentials));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        (await action.Should().ThrowAsync<BusinessException>())
            .And
            .Message.Should().Be(UserErrors.InvalidCredentials.Detail);

        _loginServiceMock.Verify(x => x.LoginAsync(
            command.Email,
            command.Password,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}