using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Exceptions;
using EasyCash.Command.Users.Register;
using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.Interfaces;
using FluentAssertions;
using Moq;

namespace EasyCash.Unit.Tests.Application.Users.RegisterUsers;

[Collection(nameof(UserTestsFixtureCollection))]
public class RegisterUserCommandHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly Mock<IRegisterService<UserEntity>> _registerServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _registerServiceMock = new Mock<IRegisterService<UserEntity>>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new RegisterUserCommandHandler(
            _registerServiceMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var command = _fixture.RegisterUserCommandFaker.Generate();
        var identityId = Guid.NewGuid().ToString();

        _registerServiceMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<UserEntity>(),
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(identityId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRegisterFails_ShouldReturnFailureResult()
    {
        // Arrange
        var command = _fixture.RegisterUserCommandFaker.Generate();
        var error = "Registration failed";

        _registerServiceMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<UserEntity>(),
                command.Password,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new Error("", error)));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        (await action.Should().ThrowAsync<BusinessException>())
            .And
            .Message.Should().Be(error);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}