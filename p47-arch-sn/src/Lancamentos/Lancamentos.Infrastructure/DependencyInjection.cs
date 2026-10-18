using FluxoCaixa.Contracts;
using Lancamentos.Application.Ports;
using Lancamentos.Infrastructure.Messaging;
using Lancamentos.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lancamentos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLancamentosInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' não configurada.");

        services.AddDbContext<LancamentosDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(LancamentosDbContext).Assembly.FullName)));

        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMassTransit(bus =>
        {
            // Outbox EF Core: a mensagem publicada é gravada na mesma TX do
            // lançamento e só vai pro broker depois do commit. Resolve o dual-write.
            bus.AddEntityFrameworkOutbox<LancamentosDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(1);
            });

            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingRabbitMq((context, cfg) =>
            {
                ConfigurarHostRabbitMq(cfg, configuration);

                // Exchange topic para o evento, permitindo novos consumidores no futuro.
                cfg.Publish<LancamentoRegistrado>(x => x.ExchangeType = "topic");

                cfg.ConfigureEndpoints(context);
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
