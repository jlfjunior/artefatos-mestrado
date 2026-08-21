using Consolidado.Application;
using Consolidado.Domain;
using Consolidado.Infrastructure;
using Consolidado.Infrastructure.Persistence;
using FluentAssertions;
using FluxoCaixa.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Consolidado.IntegrationTests;

/// <summary>
/// Sobe a infra (Postgres, RabbitMQ e Redis), o host do Consolidado (consumidor) e
/// um host produtor à parte UMA vez para a classe inteira. Subir/derrubar um bus do
/// MassTransit por teste no mesmo processo leva o consumidor seguinte a não fazer
/// bind corretamente, então a infra é compartilhada via <see cref="IClassFixture{T}"/>
/// e cada teste usa um dia distinto. A publicação sai de um bus produtor separado
/// (o serviço de Lançamentos é um processo diferente do Consolidado, afinal).
/// </summary>
public class ConsolidadoInfraFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management-alpine")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private string _postgresConn = null!;

    public IHost? Consumidor { get; private set; }
    public IHost? Produtor { get; private set; }

    public async Task InitializeAsync()
    {
        if (!DockerCheck.Disponivel())
            return;

        await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync(), _redis.StartAsync());

        _postgresConn = _postgres.GetConnectionString();
        var rabbitConn = _rabbit.GetConnectionString();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgresConn,
                ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
                ["ConnectionStrings:RabbitMq"] = rabbitConn
            })
            .Build();

        Consumidor = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(config);
                services.AddConsolidadoInfrastructure(config);
                services.AddScoped<AtualizarSaldoService>();
                services.AddScoped<ConsultarSaldoService>();
            })
            .Build();

        await Consumidor.Services.InicializarBancoConsolidadoAsync();
        await Consumidor.StartAsync();

        Produtor = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMassTransit(bus =>
                {
                    bus.UsingRabbitMq((_, cfg) =>
                    {
                        cfg.Host(new Uri(rabbitConn));
                        cfg.Publish<LancamentoRegistrado>(x => x.ExchangeType = "topic");
                    });
                });
            })
            .Build();

        await Produtor.StartAsync();

        // Dá um tempo para o binding exchange->fila do consumidor assentar antes de
        // os testes publicarem. Sem isso, publicar rápido demais logo após o startup
        // pode fazer a primeira mensagem não ser roteada (corrida só de teste).
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task<SaldoDiario?> LerSaldoAsync(DateOnly dia)
    {
        var options = new DbContextOptionsBuilder<ConsolidadoDbContext>()
            .UseNpgsql(_postgresConn)
            .Options;

        await using var db = new ConsolidadoDbContext(options);
        return await db.SaldosDiarios.AsNoTracking().FirstOrDefaultAsync(s => s.Data == dia);
    }

    public async Task DisposeAsync()
    {
        if (Produtor is not null)
        {
            await Produtor.StopAsync();
            Produtor.Dispose();
        }

        if (Consumidor is not null)
        {
            await Consumidor.StopAsync();
            Consumidor.Dispose();
        }

        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _rabbit.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask());
    }
}

/// <summary>
/// Prova o fluxo assíncrono ponta a ponta do read side com Postgres, RabbitMQ e
/// Redis reais: publica LancamentoRegistrado -> consumer processa -> projeção do
/// saldo atualizada. Inclui prova de idempotência.
/// </summary>
public class FluxoConsolidadoTests : IClassFixture<ConsolidadoInfraFixture>
{
    private readonly ConsolidadoInfraFixture _fix;

    public FluxoConsolidadoTests(ConsolidadoInfraFixture fix) => _fix = fix;

    [DockerDisponivelFact]
    public async Task Evento_publicado_deve_projetar_saldo_do_dia()
    {
        var dia = new DateOnly(2026, 6, 19);
        var bus = _fix.Produtor!.Services.GetRequiredService<IBus>();

        await bus.Publish(NovoEvento(tipo: "Credito", valor: 100m, data: dia));
        await bus.Publish(NovoEvento(tipo: "Debito", valor: 30m, data: dia));

        var saldo = await EsperarSaldoAsync(dia, esperado: 70m);

        saldo.Should().NotBeNull();
        saldo!.TotalCreditos.Should().Be(100m);
        saldo.TotalDebitos.Should().Be(30m);
        saldo.Saldo.Should().Be(70m);
    }

    [DockerDisponivelFact]
    public async Task Mesmo_evento_entregue_duas_vezes_nao_dobra_o_saldo()
    {
        var dia = new DateOnly(2026, 7, 1);
        var bus = _fix.Produtor!.Services.GetRequiredService<IBus>();

        var evento = NovoEvento("Credito", 200m, dia);

        // Mesma mensagem (mesmo LancamentoId) publicada duas vezes.
        await bus.Publish(evento);
        await bus.Publish(evento);

        var saldo = await EsperarSaldoAsync(dia, esperado: 200m);

        saldo.Should().NotBeNull();
        saldo!.Saldo.Should().Be(200m, "o consumidor é idempotente por LancamentoId");
        saldo.TotalCreditos.Should().Be(200m);
    }

    private static LancamentoRegistrado NovoEvento(string tipo, decimal valor, DateOnly data) => new()
    {
        LancamentoId = Guid.NewGuid(),
        Tipo = tipo,
        Valor = valor,
        Data = data,
        Descricao = null,
        OcorridoEmUtc = DateTime.UtcNow
    };

    /// <summary>Aguarda a projeção convergir (consistência eventual) ou estoura timeout.</summary>
    private async Task<SaldoDiario?> EsperarSaldoAsync(DateOnly dia, decimal esperado)
    {
        // Timeout folgado: sob Docker carregado (ex.: a stack do compose rodando
        // junto), a projeção pode demorar mais para convergir.
        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            var saldo = await _fix.LerSaldoAsync(dia);
            if (saldo is not null && saldo.Saldo == esperado)
                return saldo;

            await Task.Delay(500);
        }

        return await _fix.LerSaldoAsync(dia);
    }
}
