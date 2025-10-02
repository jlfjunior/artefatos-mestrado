using EasyCash.Api;
using EasyCash.Command.Store.Contexts;
using EasyCash.Dapper.Provider;
using EasyCash.Integration.Tests.Users;
using EasyCash.Keycloak.Identity.Provider.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using System.Net.Http.Json;
using EasyCash.Abstractions.Interfaces;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace EasyCash.Integration.Tests.Infrastructure;

public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("easycash")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder()
        .WithResourceMapping(
            new FileInfo(".files/easycash-realm-export.json"),
            new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            string connectionString = $"{_dbContainer.GetConnectionString()}";

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(connectionString)
                    .UseSnakeCaseNamingConvention());

            services.RemoveAll(typeof(ISqlConnectionFactory));

            services.AddSingleton<ISqlConnectionFactory>(_ =>
                new SqlConnectionFactory(connectionString));

            services.RemoveAll(typeof(IConnectionMultiplexer));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configurationOptions = ConfigurationOptions.Parse(_redisContainer.GetConnectionString());
                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectRetry = 5;

                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            services.Configure<RedisCacheOptions>(redisCacheOptions =>
                redisCacheOptions.Configuration = _redisContainer.GetConnectionString());

            string? keycloakAddress = _keycloakContainer.GetBaseAddress();

            services.Configure<KeycloakOptions>(o =>
            {
                o.AdminUrl = $"{keycloakAddress}admin/realms/easycash/";
                o.TokenUrl = $"{keycloakAddress}realms/easycash/protocol/openid-connect/token";
            });

            services.Configure<AuthenticationOptions>(o =>
            {
                o.Issuer = $"{keycloakAddress}realms/easycash/";
                o.MetadataUrl = $"{keycloakAddress}realms/easycash/.well-known/openid-configuration";
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _keycloakContainer.StartAsync();
        await InitializeTestUserAsync();
    }

    public new async Task DisposeAsync()
    {
        try
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();

        }
        catch (Exception)
        {
            // ignore
        }

        try
        {
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();

        }
        catch (Exception)
        {
            // ignore
        }

        try
        {
            await _keycloakContainer.StopAsync();
            await _keycloakContainer.DisposeAsync();

        }
        catch (Exception)
        {
            // ignore
        }

        await base.DisposeAsync();
    }

    private async Task InitializeTestUserAsync()
    {
        try
        {
            using HttpClient httpClient = CreateClient();
            await httpClient.PostAsJsonAsync("api/v1/users/register", UserData.RegisterTestUserRequest);
        }
        catch
        {
            // Log
        }
    }
}