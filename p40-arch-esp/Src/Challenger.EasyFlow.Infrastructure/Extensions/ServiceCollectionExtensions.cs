using Challenger.EasyFlow.Application.Common.EventBus;
using Challenger.EasyFlow.Application.Features.CashBoxManagement.CreateTransaction;
using Challenger.EasyFlow.Application.Features.CashBoxManagement.ListTransaction;
using Challenger.EasyFlow.Application.Features.FinancialManagement.ConsolidatedDaily;
using Challenger.EasyFlow.Domain.Aggregates.AuditLogAggregate;
using Challenger.EasyFlow.Domain.Aggregates.DailyBalanceAggregate;
using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;
using Challenger.EasyFlow.Domain.Common;
using Challenger.EasyFlow.Infrastructure.Database.SQLServer;
using Challenger.EasyFlow.Infrastructure.Database.SQLServer.Repositories;
using Challenger.EasyFlow.Infrastructure.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Challenger.EasyFlow.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationServices()
        {
            services.AddTransient<CreateTransactionCommandHandler>();
            services.AddTransient<ListTransactionQueryHandler>();
            services.AddTransient<ConsolidatedDailyCommandHandler>();

            return services;
        }

        public IServiceCollection AddPersistence(IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddKeyedScoped<List<AuditLog>>("Audit");
            services.AddDbContext<EasyFlowContext>((provider, builder) =>
            {
                builder.UseSqlServer(configuration.GetConnectionString("EasyFlowDatabase"), options =>
                 {
                     options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                 });
                builder.EnableSensitiveDataLogging(environment.IsDevelopment());
                builder.EnableDetailedErrors(environment.IsDevelopment());
                builder.AddInterceptors(new AuditInterceptor(
                    provider.GetRequiredKeyedService<List<AuditLog>>("Audit"),
                    provider.GetRequiredService<IEventBus>()));
            });
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<IDailyBalanceRepository, DailyBalanceRepository>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EasyFlowContext>());

            return services;
        }

        public IServiceCollection AddEventBus(IConfiguration configuration)
        {
            services.AddSingleton(_ => new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"]!,
                UserName = configuration["RabbitMQ:UserName"]!,
                Password = configuration["RabbitMQ:Password"]!,
                ConsumerDispatchConcurrency = 1
            });

            services.AddSingleton(provider =>
            {
                var factory = provider.GetRequiredService<ConnectionFactory>();
                return factory.CreateConnectionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            });

            services.AddSingleton<IEventBus, RabbitMQEventBus>();

            return services;
        }
    }
}