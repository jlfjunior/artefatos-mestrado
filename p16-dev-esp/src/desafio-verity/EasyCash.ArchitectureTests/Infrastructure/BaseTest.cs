using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Reflection;

namespace EasyCash.ArchitectureTests.Infrastructure;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationCommandAssembly = typeof(Command.DependencyInjection).Assembly;
    protected static readonly Assembly ApplicationQueryAssembly = typeof(Query.DependencyInjection).Assembly;
    protected static readonly List<Assembly> ApplicationAssemblies = new List<Assembly>() { ApplicationCommandAssembly, ApplicationQueryAssembly };


    protected static readonly Assembly DomainCoursesAssembly = typeof(EasyCash.Domain.DateTimeProvider).Assembly;
    protected static readonly List<Assembly> DomainAssemblies = new List<Assembly>() { DomainCoursesAssembly };

    protected static readonly Assembly InfrastructureCommandStoreAssembly = typeof(Command.Store.DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureQueryStoreAssembly = typeof(Query.Store.DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureBackgroundJobsAssembly = typeof(Background.Jobs.DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureDapperAssembly = typeof(Dapper.Provider.DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureHealthCheckAssembly = typeof(HealthCheck.Provider.DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureCacheAssembly = typeof(Redis.Provider.DependencyInjection).Assembly;
    protected static readonly List<Assembly> InfrastructureAssemblies = new List<Assembly>()
    {
        InfrastructureCommandStoreAssembly,
        InfrastructureQueryStoreAssembly,
        InfrastructureBackgroundJobsAssembly,
        InfrastructureDapperAssembly,
        InfrastructureHealthCheckAssembly,
        InfrastructureCacheAssembly

    };

    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
