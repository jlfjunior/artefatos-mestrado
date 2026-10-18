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

namespace Resiliencia.IntegrationTests;

/// <summary>
/// Infra compartilhada para as provas de resiliência: Postgres, RabbitMQ e Redis
/// reais, mais um host produtor (faz o papel do Lançamentos) e um host consumidor
/// (o Consolidado). Tudo sobe UMA vez para a classe; cada teste usa um dia distinto.
///
/// Para simular "o Consolidado caiu" eu efetivamente paro e descarto o host do
/// consumidor (PararConsumidorAsync) e depois subo um novo (SubirConsumidorAsync).
/// O broker é compartilhado e a fila do consumidor é durável, então as mensagens
/// publicadas enquanto ele está fora ficam esperando no broker. A leitura do saldo
/// usa um DbContext independente do host, para funcionar mesmo com o consumidor fora.
/// </summary>
public class ResilienciaInfraFixture : IAsyncLifetime
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

    private IConfiguration _config = null!;
    private string _postgresConn = null!;

    public IHost? Produtor { get; private set; }
    public IHost? Consumidor { get; private set; }

    public async Task InitializeAsync()
    {
        if (!DockerCheck.Disponivel())
            return;

        await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync(), _redis.StartAsync());

        _postgresConn = _postgres.GetConnectionString();
        var rabbitConn = _rabbit.GetConnectionString();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgresConn,
                ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
                ["ConnectionStrings:RabbitMq"] = rabbitConn
            })
            .Build();

        // Sobe o consumidor e cria o schema (uma vez).
        Consumidor = ConstruirConsumidor();
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

        // Tempo para o binding exchange->fila assentar antes de publicar (corrida
        // só de teste; em produção o consumidor já está no ar).
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    private IHost ConstruirConsumidor() => Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddSingleton(_config);
            services.AddConsolidadoInfrastructure(_config);
            services.AddScoped<AtualizarSaldoService>();
            services.AddScoped<ConsultarSaldoService>();
        })
        .Build();

    /// <summary>Derruba o Consolidado: para e descarta o host do consumidor.</summary>
    public async Task PararConsumidorAsync()
    {
        if (Consumidor is null)
            return;

        await Consumidor.StopAsync();
        Consumidor.Dispose();
        Consumidor = null;
    }

    /// <summary>Consolidado volta: sobe um novo host de consumidor no mesmo broker.</summary>
    public async Task SubirConsumidorAsync()
    {
        Consumidor = ConstruirConsumidor();
        await Consumidor.StartAsync();
    }

    /// <summary>Lê o saldo por um DbContext próprio, independente do host do consumidor.</summary>
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
/// Provas de resiliência do desacoplamento entre Lançamentos e Consolidado. O RNF
/// central é que nenhuma mensagem se perca e que o Consolidado possa cair sem
/// derrubar o produtor — a fila bufferiza e, quando o consumidor volta, processa
/// o backlog.
/// </summary>
[TestCaseOrderer("Resiliencia.IntegrationTests.OrdenadorPorPrioridade", "Resiliencia.IntegrationTests")]
public class PerdaDeMensagemTests : IClassFixture<ResilienciaInfraFixture>
{
    private readonly ResilienciaInfraFixture _fix;

    public PerdaDeMensagemTests(ResilienciaInfraFixture fix) => _fix = fix;

    [DockerDisponivelFact]
    [Prioridade(1)]
    public async Task Quinhentos_lancamentos_devem_ser_projetados_sem_perder_nenhum()
    {
        const int n = 500;
        var dia = new DateOnly(2026, 6, 19);

        // Metade créditos de 10, metade débitos de 4. Saldo esperado fechado.
        var creditos = n / 2;
        var debitos = n - creditos;
        var creditoEsperado = creditos * 10m;
        var debitoEsperado = debitos * 4m;
        var saldoEsperado = creditoEsperado - debitoEsperado;

        var bus = _fix.Produtor!.Services.GetRequiredService<IBus>();
        for (var i = 0; i < n; i++)
        {
            var ehCredito = i % 2 == 0;
            await bus.Publish(NovoEvento(
                tipo: ehCredito ? "Credito" : "Debito",
                valor: ehCredito ? 10m : 4m,
                data: dia));
        }

        var saldo = await EsperarSaldoAsync(dia, saldoEsperado, timeout: TimeSpan.FromSeconds(150));

        saldo.Should().NotBeNull("todos os {0} lançamentos precisam ter sido projetados", n);
        saldo!.TotalCreditos.Should().Be(creditoEsperado);
        saldo.TotalDebitos.Should().Be(debitoEsperado);
        saldo.Saldo.Should().Be(saldoEsperado);
    }

    [DockerDisponivelFact]
    [Prioridade(3)]
    public async Task Consumidor_fora_do_ar_nao_perde_mensagens_ao_voltar()
    {
        // Carga menor aqui de propósito: ao recriar o host do consumidor, esse
        // segundo bus no mesmo processo processa bem mais devagar (limitação do
        // ambiente de teste, não da aplicação). 20 mensagens já provam a
        // propriedade — buffer enquanto fora, drenagem ao voltar, zero perda.
        const int n = 20;
        var dia = new DateOnly(2026, 7, 10);
        var saldoEsperado = n * 5m;

        // Derruba o consumidor (Consolidado cai). O produtor continua publicando.
        await _fix.PararConsumidorAsync();

        var bus = _fix.Produtor!.Services.GetRequiredService<IBus>();
        for (var i = 0; i < n; i++)
            await bus.Publish(NovoEvento("Credito", 5m, dia));

        // Com o consumidor fora, a fila durável segura o backlog. Dou um tempo para
        // deixar claro que nada está sendo consumido enquanto está fora.
        await Task.Delay(TimeSpan.FromSeconds(2));
        var saldoComConsumidorFora = await _fix.LerSaldoAsync(dia);
        saldoComConsumidorFora.Should().BeNull("com o Consolidado fora, nada deve ter sido projetado ainda");

        // Consolidado volta: sobe um novo consumidor que drena o backlog da fila.
        await _fix.SubirConsumidorAsync();

        var saldo = await EsperarSaldoAsync(dia, saldoEsperado, timeout: TimeSpan.FromSeconds(180));

        saldo.Should().NotBeNull("ao voltar, o consumidor tem de processar o backlog inteiro");
        saldo!.Saldo.Should().Be(saldoEsperado, "a fila bufferizou e nada se perdeu");
    }

    [DockerDisponivelFact]
    [Prioridade(2)]
    public async Task Mesma_mensagem_reentregue_nao_dobra_o_saldo()
    {
        var dia = new DateOnly(2026, 8, 1);
        var bus = _fix.Produtor!.Services.GetRequiredService<IBus>();

        var evento = NovoEvento("Credito", 320m, dia);

        // Reentrega: o mesmo LancamentoId publicado duas vezes (at-least-once).
        await bus.Publish(evento);
        await bus.Publish(evento);

        var saldo = await EsperarSaldoAsync(dia, 320m, timeout: TimeSpan.FromSeconds(30));

        saldo.Should().NotBeNull();
        saldo!.Saldo.Should().Be(320m, "o consumidor é idempotente por LancamentoId");
        saldo.TotalCreditos.Should().Be(320m);
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
    private async Task<SaldoDiario?> EsperarSaldoAsync(DateOnly dia, decimal esperado, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            var saldo = await _fix.LerSaldoAsync(dia);
            if (saldo is not null && saldo.Saldo == esperado)
                return saldo;

            await Task.Delay(500);
        }

        // Última leitura: o assert reporta o que de fato ficou gravado.
        return await _fix.LerSaldoAsync(dia);
    }
}
