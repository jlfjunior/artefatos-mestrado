using FluxoDeCaixa.Caixa;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluxoDeCaixa.Lancamentos.API.Tests.Config
{
    public class IntegrationTestsFixture<TStartup> : IDisposable where TStartup : class
    {
        [CollectionDefinition(nameof(IntegrationApiTestsFixtureCollection))]
        public class IntegrationApiTestsFixtureCollection : ICollectionFixture<IntegrationTestsFixture<StartupApiTests>> { }

        public readonly FluxoDeCaixaAppFactory<TStartup> Factory;
        public HttpClient Client;
        public string Token;

        public IntegrationTestsFixture()
        {
            var clientOptions = new WebApplicationFactoryClientOptions();

            Factory = new FluxoDeCaixaAppFactory<TStartup>();
            Client = Factory.CreateClient();
        }

        public async Task LoginApi()
        {
            var userData = new Login
            {
                Email = "b20-ferraz@hotmail.com",
                Password = "Password@123"
            };

            // Recriando o client para evitar configurações de Web
            Client = Factory.CreateClient();
            
            var response = await new HttpClient().PostAsJsonAsync("http://localhost:5003/auth/api/identidade/login", userData);
            response.EnsureSuccessStatusCode();
            Token = await response.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            Factory?.Dispose();
            Client?.Dispose();
        }
    }

    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
