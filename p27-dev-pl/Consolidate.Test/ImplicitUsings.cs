global using FluentAssertions;
global using TechTalk.SpecFlow;
using Foundation.Business.DomainNotitications;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using SolidToken.SpecFlow.DependencyInjection;
using Consolidate.Business.Contracts;
using Consolidate.Data.Repository;
using MongoDB.Driver;
using Consolidate.Data.Mapping;
using Consolidate.Business.UseCase.AddConsolidateTransaction;

namespace Consolidate.Test
{
    public static class TestDependencies
    {
        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services
            .AddLogging()
            .AddAutoMapper(typeof(MappingProfile))
            .AddScoped<DomainNotificationContext, TestDomainNotificationContext>()
            .AddScoped<IConsolidateTransactionRepository, ConsolidateTransactionMongoRepository>()
            .AddSingleton<IMongoClient>(new MongoClient("mongodb://admin:secretpassword@localhost:27017/?authSource=admin&readPreference=primary&ssl=false&directConnection=true"))            
            .AddScoped<IMongoDatabase>(provider =>
            {
                var client = provider.GetService<IMongoClient>();
                return client.GetDatabase("Transaction");
            });

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(AddConsolidateTransactionInputDto).Assembly, typeof(AddConsolidateTransaction).Assembly);

            });

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
            // TODO: add your test dependencies here

            return services;
        }
    }

}