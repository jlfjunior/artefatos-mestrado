using EasyCash.Command.Transactions.Create;
using EasyCash.Integration.Tests.Infrastructure;
using EasyCash.Shared.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EasyCash.Integration.Tests.Transactions;

public sealed class CreateTransactionTests : BaseFunctionalTest
{
    public CreateTransactionTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnOk_WhenIsValidRequest()
    {
        // Arrange
        string accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken);

        var request = new CreateTransactionCommand()
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            Amount = 100,
            Description = "Test",
            Category = "Test",
            Date = DateTime.Now,
            Type = ETransactionType.Credit
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/v1/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

