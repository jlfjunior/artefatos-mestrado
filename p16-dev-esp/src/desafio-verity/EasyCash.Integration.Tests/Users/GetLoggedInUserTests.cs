using EasyCash.Integration.Tests.Infrastructure;
using EasyCash.Query.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EasyCash.Integration.Tests.Users;

public class GetLoggedInUserTests : BaseFunctionalTest
{
    public GetLoggedInUserTests(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Get_ShouldReturnUnauthorized_WhenAccessTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ShouldReturnUser_WhenAccessTokenIsNotMissing()
    {
        // Arrange
        string accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken);

        // Act
        UserQueryResult? user = await HttpClient.GetFromJsonAsync<UserQueryResult>("api/v1/users/me");

        // Assert
        user.Should().NotBeNull();
    }
}
