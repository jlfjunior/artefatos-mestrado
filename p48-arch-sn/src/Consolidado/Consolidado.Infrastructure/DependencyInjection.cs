using Consolidado.Application.Ports;
using Consolidado.Infrastructure.Caching;
using Consolidado.Infrastructure.Messaging;
using Consolidado.Infrastructure.Persistence;
using FluxoCaixa.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consolidado.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddConsolidadoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' não configurada.");

        services.AddDbContext<ConsolidadoDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ISaldoDiarioRepository, SaldoDiarioRepository>();
        services.AddScoped<ILancamentosProcessadosStore, LancamentosProcessadosStore>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ISaldoCache, RedisSaldoCache>();

        services.AddStackExchangeRedisCache(o =>
        {
            o.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            o.InstanceName = "consolidado:";
        });

        services.AddMassTransit(bus =>
        {
            // O Consolidado não publica mensagens (só consome), e a idempotência já
            // é garantida pela chave de negócio LancamentoId na tabela
            // lancamentos_processados, dentro da mesma transação da projeção. Por
            // isso não uso o inbox/outbox do MassTransit aqui — seria redundante e
            // adiciona overhead no consumo.
            bus.AddConsumer<LancamentoRegistradoConsumer>();
            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingRabbitMq((context, cfg) =>
            {
                ConfigurarHostRabbitMq(cfg, configuration);

                cfg.Publish<LancamentoRegistrado>(x => x.ExchangeType = "topic");

                cfg.ReceiveEndpoint("consolidado-saldo-diario", e =>
                {
                    // A projeção do saldo é keyed pela data: uma linha por dia.
                    // Processo as mensagens em série (single-writer) para evitar
                    // corrida de escrita na mesma linha — dois eventos do mesmo dia
                    // tentando inserir/atualizar o mesmo registro ao mesmo tempo
                    // resultariam em violação de PK. O requisito de 50 req/s é do
                    // lado de LEITURA do consolidado, não deste consumidor, então
                    // serializar aqui não conflita com a meta. Escala-out futura:
                    // particionar o consumo por data.
                    e.ConcurrentMessageLimit = 1;

                    // Retry com backoff exponencial para falhas transientes
                    // (Redis/DB momentaneamente fora). Esgotadas as tentativas, a
                    // mensagem vai para a _error queue (DLQ) que o MassTransit
                    // cria automaticamente para o endpoint.
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(2)));

                    e.ConfigureConsumer<LancamentoRegistradoConsumer>(context);
                });
            });
        });

        return services;
    }

    /// <summary>
    /// Configura o host do RabbitMQ. Aceita uma URI AMQP completa via
    /// ConnectionStrings:RabbitMq (amqp://user:senha@host:porta) — útil quando a
    /// porta não é a padrão (Testcontainers, CloudAMQP) — e, na ausência dela,
    /// cai para as chaves avulsas em RabbitMq:* (cenário do docker-compose).
    /// </summary>
    private static void ConfigurarHostRabbitMq(IRabbitMqBusFactoryConfigurator cfg, IConfiguration configuration)
    {
        var uri = configuration.GetConnectionString("RabbitMq");
        if (!string.IsNullOrWhiteSpace(uri))
        {
            cfg.Host(new Uri(uri));
            return;
        }

        var rabbit = configuration.GetSection("RabbitMq");
        cfg.Host(rabbit["Host"] ?? "localhost", rabbit["VirtualHost"] ?? "/", h =>
        {
            h.Username(rabbit["Username"] ?? "guest");
            h.Password(rabbit["Password"] ?? "guest");
        });
    }
}
