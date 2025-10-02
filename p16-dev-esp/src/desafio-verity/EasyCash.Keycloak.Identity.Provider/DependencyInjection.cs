using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Authorization;
using EasyCash.Domain.Users.Entities;
using EasyCash.Keycloak.Identity.Provider.Authentication;
using EasyCash.Keycloak.Identity.Provider.Login;
using EasyCash.Keycloak.Identity.Provider.Register;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EasyCash.Keycloak.Identity.Provider;

public static class DependencyInjection
{
    public static void AddInfrastructureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorization(options =>
            {
                options.AddPolicy(PoliciesConsts.CollaboratorUser, builder =>
                {
                    builder
                        .RequireRealmRoles(PermissionsConsts.Collaborator); // Realm role is fetched from token
                    // .RequireResourceRoles("Admin"); // Resource/Client role is fetched from token
                });
            })
            .AddKeycloakAuthorization(configuration);

        services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));

        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));

        services.AddTransient<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient<IRegisterService<UserEntity>, RegisterService>((serviceProvider, httpClient) =>
            {
                KeycloakOptions keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

                httpClient.BaseAddress = new Uri(keycloakOptions.AdminUrl);
            })
            .AddHttpMessageHandler<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient<ILoginService, LoginService>((serviceProvider, httpClient) =>
        {
            KeycloakOptions keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

            httpClient.BaseAddress = new Uri(keycloakOptions.TokenUrl);
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
    }
}

internal sealed class RealmAccess
{
    public List<string> Roles { get; set; }
}