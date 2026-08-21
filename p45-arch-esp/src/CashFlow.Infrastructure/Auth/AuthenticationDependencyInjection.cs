using CashFlow.Application.Abstractions;
using CashFlow.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Registro de autenticação JWT e serviços OAuth2.
/// </summary>
public static class AuthenticationDependencyInjection
{
    /// <summary>
    /// Registra autenticação JWT, contexto de usuário e serviços OAuth2.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <returns>Coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddCashFlowAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OAuthOptions>(configuration.GetSection(OAuthOptions.SectionName))
            .AddHttpContextAccessor()
            .AddScoped<ICurrentUserContext, HttpCurrentUserContext>()
            .AddSingleton<SigningKeysService>()
            .AddSingleton<JwtSigningKeyResolver>()
            .AddScoped<OAuthTokenService>()
            .AddHostedService<SigningKeyBootstrapHostedService>();

        var oauth = configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>() ?? new();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = oauth.Issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context => Task.CompletedTask,
                    OnTokenValidated = context => Task.CompletedTask
                };
            });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtSigningKeyResolver>((options, resolver) =>
            {
                options.TokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    resolver.ResolveAsync(kid).GetAwaiter().GetResult();
            });

        return services;
    }
}
