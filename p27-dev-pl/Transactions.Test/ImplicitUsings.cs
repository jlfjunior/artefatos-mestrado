global using FluentAssertions;
global using TechTalk.SpecFlow;
using Foundation.Business.DomainNotitications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SolidToken.SpecFlow.DependencyInjection;
using Transactions.Business.Contracts;
using Transactions.Business.UseCase;
using Transactions.Data.Mapping;
using Transactions.Data.Repository;
using Transactions.Test;

public static class TestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services
            .AddLogging()
            .AddScoped<DomainNotificationContext, TestDomainNotificationContext>()
            .AddScoped<ITransactionRepository, TransactionMongoRepository>()
            .AddAutoMapper(typeof(MappingProfile))
            .AddScoped<ITransactionRepository, TransactionMongoRepository>()
            .AddSingleton<IMongoClient>(new MongoClient("mongodb://admin:secretpassword@localhost:27017/?authSource=admin&readPreference=primary&ssl=false&directConnection=true"))
            .AddScoped<IMongoDatabase>(provider =>
            {
                var client = provider.GetService<IMongoClient>();
                return client.GetDatabase("Consolidate");
            }); ;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(TransactionInputDto).Assembly, typeof(AddTransactionUseCase).Assembly);

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


