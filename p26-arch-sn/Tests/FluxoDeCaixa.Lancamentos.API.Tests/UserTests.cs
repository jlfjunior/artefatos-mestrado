using FluxoDeCaixa.Caixa;
using FluxoDeCaixa.Lancamentos.API.Tests.Config;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FluxoDeCaixa.Lancamentos.API.Tests.Config.IntegrationTestsFixture<FluxoDeCaixa.Caixa.StartupApiTests>;

namespace FluxoDeCaixa.Lancamentos.API.Tests
{
    [Collection(nameof(IntegrationApiTestsFixtureCollection))]
    public class UserTests
    {
        private readonly IntegrationTestsFixture<StartupApiTests> _testsFixture;

        public UserTests(IntegrationTestsFixture<StartupApiTests> testsFixture)
        {
            _testsFixture = testsFixture;
        }

        [Fact(DisplayName = "Teste")]
        public async Task Teste()
        {
            // Arrange
            //var initialResponse = await _testsFixture.Client.GetAsync("/api/account/test");
            //initialResponse.EnsureSuccessStatusCode();

            await _testsFixture.LoginApi();

            _testsFixture.Client.AtribuirToken(_testsFixture.Token);
            var initialResponse = await _testsFixture.Client.GetAsync("/api/account/show-info");
            initialResponse.EnsureSuccessStatusCode();

        }
    }
}
