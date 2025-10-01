using Bogus;
using CashFlow.AppHost.EndToEndTests.Fixture;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using System.Net.Http.Json;

namespace CashFlow.AppHost.EndToEndTests
{
    [Collection("CashFlowApp")]
    public class UserTests
    {
        private readonly CashFlowAppsFixture _fixture;

        public UserTests(CashFlowAppsFixture appsFixture)
        {
            _fixture = appsFixture;
        }

        [Fact]
        public async Task RegisterNewUser_ReturnOk()
        {
            // arrange
            var request = new RegisterRequest()
            {
                Email = "ola@test.com",
                Password = "T3ste@144"
            };

            // act
            var response =
               await _fixture.ClientIdentityServerApi().PostAsJsonAsync("/register", request);

            /// assets
            response.Should().Be200Ok();
        }

        [Fact]
        public async Task LoginInvalidUser_ReturnNotUnauthorized()
        {
            // arrange
            var request = new RegisterRequest()
            {
                Email = "usuario-invalido@google.com",
                Password = Guid.NewGuid().ToString()
            };

            // act
            var response =
               await _fixture.ClientIdentityServerApi().PostAsJsonAsync("/login", request);

            /// assets
            response.Should().Be401Unauthorized();
        }

        [Fact]
        public async Task LoginForNewUser_ReturnAcessToken()
        {
            var responseToken =
                await GetTokenForNewUserAsync(_fixture.ClientIdentityServerApi());

            responseToken!.AccessToken.Should().NotBeNullOrEmpty();
        }

        public static async Task<ResponseUserToken?> GetTokenForNewUserAsync(HttpClient httpClient)
        {
            var registerRequest =
            new Faker<RegisterRequest>()
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.Password, f => f.Internet.Password(8, false, "\\w", "@1Aa@a"))
                .Generate();

            var registerResponse = await httpClient.PostAsJsonAsync("/register", registerRequest);                        
            var loginResponse = await httpClient.PostAsJsonAsync("/login", registerRequest);
            var responseToken = await loginResponse.Content.ReadFromJsonAsync<ResponseUserToken>();           

            return responseToken;
        }

        public class ResponseUserToken
        {
            public string AccessToken { get; set; } = string.Empty;
            public int ExpiresIn { get; set; }
        }
    }
}
