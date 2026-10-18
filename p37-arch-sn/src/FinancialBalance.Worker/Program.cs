using FinancialBalance.Application.Common;
using FinancialBalance.Domain.Reporting;
using FinancialBalance.Worker.Consumers;
using FinancialBalance.Worker.Infrastructure.Cache;
using FinancialBalance.Worker.Infrastructure.Persistence;
using FinancialBalance.Worker.Infrastructure.Persistence.Repositories;
using FinancialBalance.Worker.Jobs;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console())
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;

        // EF Core — reporting schema
        // MaxPoolSize appended to connection string; CommandTimeout and retry via EF options
        var pgConn = config.GetConnectionString("Postgres")!;
        if (!pgConn.Contains("MaxPoolSize", StringComparison.OrdinalIgnoreCase))
            pgConn += ";MaxPoolSize=50";

        services.AddDbContext<WorkerDbContext>(options =>
            options.UseNpgsql(
                pgConn,
                npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_reporting_migrations", "reporting");
                    npgsql.CommandTimeout(10);
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                }));

        // Repositories
        services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();
        services.AddScoped<IMonthlySummaryRepository, MonthlySummaryRepository>();

        // Redis cache
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(config.GetConnectionString("Redis")!);
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;
            options.AbortOnConnectFail = false;
            options.ReconnectRetryPolicy = new ExponentialRetry(1000, 10000);
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddScoped<IReportCache, RedisReportCache>();

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TransactionCreatedConsumer, TransactionCreatedConsumerDefinition>();
            x.AddConsumer<TransactionCancelledConsumer, TransactionCancelledConsumerDefinition>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(config["RabbitMQ:Host"], h =>
                {
                    h.Username(config["RabbitMQ:Username"]!);
                    h.Password(config["RabbitMQ:Password"]!);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        // Background jobs
        services.AddHostedService<MonthlyRollupJob>();
        services.AddHostedService<DailySummaryCleanupJob>();

        // OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("financial-balance-reporting-worker"))
            .WithTracing(t => t
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource("MassTransit"));
    })
    .Build();

// Apply pending EF Core migrations on startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
