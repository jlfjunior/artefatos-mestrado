using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CashFlow.Application.Commands;
using CashFlow.Application.CoR;
using CashFlow.Application.Queries;
using CashFlow.Application.UseCases;
using CashFlow.Domain.Interfaces;
using CashFlow.Domain.Services;
using CashFlow.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;

namespace CashFlow.API.Configurations;

[ExcludeFromCodeCoverage]
public static class IoCExtension
{
    public static IServiceCollection IoC(this IServiceCollection services)
    {
        return services
            .AddInfrastructure()
            .DomainService()
            .Application();
    }


    private static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<ICashFlowRepository, MongoCashFlowRepository>();

        return services;
    }

    private static IServiceCollection DomainService(this IServiceCollection services)
    {
        services.AddScoped<ICashFlowService, CashFlowService>();

        return services;
    }

    private static IServiceCollection Application(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services
            .AddSingleton<IValidator<AddTransactionDailyCommand>, AddTransactionCommandValidator>()
            .AddSingleton<IPipelineBehavior<RegisterNewCashFlowCommand, CommandResponse<Guid>>,
                ValidateCommandCoR<RegisterNewCashFlowCommand, Guid>>()
            .AddTransient<IRequestHandler<AddTransactionDailyCommand, CommandResponse<Guid>>, AddTransactionUseCase>();


        services
            .AddSingleton<IValidator<CancelTransactionCommand>, CancelTransactionCommandValidator>()
            .AddSingleton<IPipelineBehavior<CancelTransactionCommand, CommandResponse<Guid>>,
                ValidateCommandCoR<CancelTransactionCommand, Guid>>()
            .AddTransient<IRequestHandler<CancelTransactionCommand, CommandResponse<Guid>>, CancelTransactionUseCase>();

        services
            .AddSingleton<IValidator<RegisterNewCashFlowCommand>, RegisterNewCashFlowCommandValidator>()
            .AddSingleton<IPipelineBehavior<RegisterNewCashFlowCommand, CommandResponse<Guid>>,
                ValidateCommandCoR<RegisterNewCashFlowCommand, Guid>>()
            .AddTransient<IRequestHandler<RegisterNewCashFlowCommand, CommandResponse<Guid>>,
                RegisterNewCashFlowUseCase>();


        services
            .AddSingleton<IValidator<GetDailyBalanceQuery>, GetDailyBalanceQueryValidator>()
            .AddSingleton<IPipelineBehavior<GetDailyBalanceQuery, CommandResponse<GetDailyBalanceQueryResponse>>,
                ValidateCommandCoR<GetDailyBalanceQuery, GetDailyBalanceQueryResponse>>()
            .AddTransient<IRequestHandler<GetDailyBalanceQuery, CommandResponse<GetDailyBalanceQueryResponse>>,
                GetDailyBalanceUseCase>();

        services
            .AddSingleton<IValidator<GetByAccountIdAndDateRangeQuery>, GetByAccountIdAndDateRangeQueryValidator>()
            .AddSingleton<
                IPipelineBehavior<GetByAccountIdAndDateRangeQuery,
                    CommandResponse<GetByAccountIdAndDateRangeQueryResponse>>, ValidateCommandCoR<
                    GetByAccountIdAndDateRangeQuery, GetByAccountIdAndDateRangeQueryResponse>>()
            .AddTransient<IRequestHandler<GetByAccountIdAndDateRangeQuery,
                    CommandResponse<GetByAccountIdAndDateRangeQueryResponse>>,
                GetByAccountIdAndDateRangeUseCase>();

        return services;
    }
}