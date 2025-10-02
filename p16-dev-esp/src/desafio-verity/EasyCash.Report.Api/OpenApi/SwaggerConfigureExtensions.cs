using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace EasyCash.Report.Api.OpenApi;

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
        });

        return services;
    }
}