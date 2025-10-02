using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace EasyCash.Api.OpenApi;

internal static class SwaggerConfigureExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            var jwtSecurityScheme =
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Authorization token put **_ONLY_** your JWT Bearer token on textbox below!",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

            o.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            o.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme,
                        new List<string>()
                    }
                });

            //o.AddSecurityDefinition("keycloak", new OpenApiSecurityScheme()
            //{
            //    Type = SecuritySchemeType.OAuth2,
            //    Flows = new OpenApiOAuthFlows
            //    {
            //        Implicit = new OpenApiOAuthFlow
            //        {
            //            AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationUrl"]!),
            //            Scopes = new Dictionary<string, string>
            //            {
            //                { "openid", "openid" },
            //                { "profile", "profile" },
            //            }
            //        }
            //    }
            //});

            //o.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Id = "keycloak",
            //                Type = ReferenceType.SecurityScheme
            //            },
            //            In = ParameterLocation.Header,
            //            Name = "Bearer",
            //            Scheme = "Bearer"
            //        },
            //        new [] { "openid", "profile" }
            //    }
            //});

        });

        return services;
    }
}