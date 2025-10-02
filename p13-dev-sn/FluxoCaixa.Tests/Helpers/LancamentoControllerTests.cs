using System.Net.Http.Json;
using FluxoCaixa.Application.Commands;
using FluxoCaixa.Domain.Enums;
using FluxoCaixa.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace FluxoCaixa.Tests.Integration;

public class LancamentoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LancamentoControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_DeveCriarLancamento_ComSucesso()
    {

        var command = new CriarLancamentoCommand
        {
            Data = DateTime.UtcNow,
            Valor = 250.00m,
            Tipo = TipoLancamento.Debito
        };


        var response = await _client.PostAsJsonAsync("/api/lancamentos", command);


        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
