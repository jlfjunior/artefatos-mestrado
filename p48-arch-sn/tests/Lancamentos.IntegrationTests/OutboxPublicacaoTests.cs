using FluentAssertions;
using FluxoCaixa.Contracts;
using Lancamentos.Application;
using Lancamentos.Application.Ports;
using Lancamentos.Domain;
using Lancamentos.Infrastructure;
using Lancamentos.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Lancamentos.IntegrationTests;

/// <summary>
/// Prova o write side: registrar lançamento persiste o agregado e, pela outbox,
/// publica o LancamentoRegistrado no RabbitMQ. Um consumer de teste confirma a
/// chegada. Postgres e RabbitMQ reais via Testcontainers.
/// </summary>
public class OutboxPublicacaoTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management-alpine")
        .Build();

    private IHost? _host;

    public async Task InitializeAsync()
    {
        if (!DockerCheck.Disponivel())
            return;

        await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync());

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["ConnectionStrings:RabbitMq"] = _rabbit.GetConnectionString()
            })
            .Build();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(config);
                services.AddLancamentosInfrastructure(config);
                services.AddScoped<RegistrarLancamentoService>();
                services.AddSingleton<EventoColetor>();
            })
            .Build();

        await _host.Services.InicializarBancoLancamentosAsync();
        await _host.StartAsync();

        // Conecta um endpoint de observação ao bus já em execução para capturar
        // o evento que a outbox publica. Bind no mesmo exchange topic do produtor.
        var bus = _host.Services.GetRequiredService<IBusControl>();
        var coletor = _host.Services.GetRequiredService<EventoColetor>();
        bus.ConnectReceiveEndpoint("teste-observador-lancamentos", e =>
        {
            e.Handler<LancamentoRegistrado>(ctx =>
            {
                coletor.Recebido(ctx.Message);
                return Task.CompletedTask;
            });
        });
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _rabbit.DisposeAsync().AsTask());
    }

    [DockerDisponivelFact]
    public async Task Registrar_lancamento_persiste_e_publica_pela_outbox()
    {
        using var scope = _host!.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<RegistrarLancamentoService>();

        var id = await service.ExecutarAsync(
            new RegistrarLancamentoCommand(TipoLancamento.Credito, 250m, new DateOnly(2026, 6, 19), "venda balcão"));

        // Persistiu no banco.
        var db = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();
        var gravado = await db.Lancamentos.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        gravado.Should().NotBeNull();
        gravado!.Valor.Should().Be(250m);

        // A outbox entregou o evento ao broker (consistência eventual).
        var coletor = _host.Services.GetRequiredService<EventoColetor>();
        var evento = await coletor.EsperarAsync(id, TimeSpan.FromSeconds(30));

        evento.Should().NotBeNull();
        evento!.Valor.Should().Be(250m);
        evento.Tipo.Should().Be("Credito");
    }
}

internal class EventoColetor
{
    private readonly TaskCompletionSource<LancamentoRegistrado> _tcs = new();
    private Guid _esperado;

    public void Aguardar(Guid id) => _esperado = id;

    public void Recebido(LancamentoRegistrado evento)
    {
        if (evento.LancamentoId == _esperado)
            _tcs.TrySetResult(evento);
    }

    public async Task<LancamentoRegistrado?> EsperarAsync(Guid id, TimeSpan timeout)
    {
        Aguardar(id);
        var concluido = await Task.WhenAny(_tcs.Task, Task.Delay(timeout));
        return concluido == _tcs.Task ? _tcs.Task.Result : null;
    }
}
