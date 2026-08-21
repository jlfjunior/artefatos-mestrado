using CashFlow.Application.Abstractions;
using CashFlow.Application.Exceptions;
using CashFlow.Application.Services;
using CashFlow.Domain.Entities;
using CashFlow.Shared.Authorization;
using CashFlow.Shared.Enums;
using FluentAssertions;
using Moq;

namespace CashFlow.Application.Tests;

public class ClientServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICurrentUserContext> _currentUser = new();
    private readonly ClientService _sut;

    public ClientServiceTests()
    {
        _sut = new ClientService(_users.Object, _currentUser.Object);
    }

    [Fact]
    public async Task ListClientsAsync_ShouldReturnMappedClients_WhenEmployee()
    {
        var client = CreateClient();
        _currentUser.SetupGet(u => u.IsEmployee).Returns(true);
        _users.Setup(u => u.ListClientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { client });

        var result = await _sut.ListClientsAsync();

        result.Should().ContainSingle()
            .Which.Email.Should().Be(client.Email);
    }

    [Fact]
    public async Task ListClientsAsync_ShouldThrowForbidden_WhenNotEmployee()
    {
        _currentUser.SetupGet(u => u.IsEmployee).Returns(false);

        var act = () => _sut.ListClientsAsync();

        await act.Should().ThrowAsync<ForbiddenException>()
            .Where(ex => ex.Error == "employee_required");
    }

    [Fact]
    public async Task GetClientAsync_ShouldReturnClient_WhenEmployeeRequestsValidClient()
    {
        var client = CreateClient();
        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _users.Setup(u => u.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var result = await _sut.GetClientAsync(client.Id);

        result.Should().NotBeNull();
        result!.FullName.Should().Be(client.FullName);
    }

    [Fact]
    public async Task GetClientAsync_ShouldThrowForbidden_WhenClientRequestsOtherProfile()
    {
        _currentUser.SetupGet(u => u.IsClient).Returns(true);
        _currentUser.SetupGet(u => u.UserId).Returns(SeedIdentifiers.ClientUserId);

        var act = () => _sut.GetClientAsync(SeedIdentifiers.EmployeeUserId);

        await act.Should().ThrowAsync<ForbiddenException>()
            .Where(ex => ex.Error == "access_denied");
    }

    [Fact]
    public async Task GetClientAsync_ShouldReturnNull_WhenUserIsNotClient()
    {
        var employee = new User
        {
            Id = SeedIdentifiers.EmployeeUserId,
            Email = SeedIdentifiers.EmployeeEmail,
            FullName = "Funcionário",
            Role = UserRole.Employee,
            Enabled = true,
            CreatedAt = DateTime.UtcNow
        };

        _currentUser.SetupGet(u => u.IsClient).Returns(false);
        _users.Setup(u => u.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.GetClientAsync(employee.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentClientAsync_ShouldDelegateToGetClientAsync()
    {
        var client = CreateClient();
        _currentUser.SetupGet(u => u.IsClient).Returns(true);
        _currentUser.SetupGet(u => u.UserId).Returns(client.Id);
        _users.Setup(u => u.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var result = await _sut.GetCurrentClientAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(client.Id);
    }

    private static User CreateClient() => new()
    {
        Id = SeedIdentifiers.ClientUserId,
        Email = SeedIdentifiers.ClientEmail,
        FullName = "Cliente Demo",
        Role = UserRole.Client,
        Enabled = true,
        CreatedAt = DateTime.UtcNow
    };
}
