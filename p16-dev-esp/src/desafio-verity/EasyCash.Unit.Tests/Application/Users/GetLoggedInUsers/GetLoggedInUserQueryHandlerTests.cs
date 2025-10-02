using EasyCash.Abstractions.Authentication;
using EasyCash.Query.Users;
using EasyCash.Query.Users.Interfaces;
using FluentAssertions;
using Moq;

namespace EasyCash.Unit.Tests.Application.Users.GetLoggedInUsers;

[Collection(nameof(UserTestsFixtureCollection))]
public class GetLoggedInUserQueryHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IUserQueryStore> _userQueryStoreMock;
    private readonly GetLoggedInUserQueryHandler _handler;

    public GetLoggedInUserQueryHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _userContextMock = new Mock<IUserContext>();
        _userQueryStoreMock = new Mock<IUserQueryStore>();
        _handler = new GetLoggedInUserQueryHandler(_userContextMock.Object, _userQueryStoreMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidIdentityId_ShouldReturnUserData()
    {
        // Arrange
        var identityId = Guid.NewGuid().ToString();
        var expectedUser = _fixture.UserQueryResultFaker.Generate();

        _userContextMock.Setup(x => x.IdentityId).Returns(identityId);
        _userQueryStoreMock
            .Setup(x => x.GetLoggedInUser(identityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.Handle(new GetLoggedInUserQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);

        _userContextMock.Verify(x => x.IdentityId, Times.Once);
        _userQueryStoreMock.Verify(x => x.GetLoggedInUser(identityId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
