using CashFlow.Application.Transactions.Handlers;
using CashFlow.Infrastructure;
using CashFlow.Infrastructure.Common.PubSub;
using FluentValidation;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace CashFlow.Api;

public static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.Services
            .ConfigureEndpoints();
        
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });
        builder.Services.AddAuthorization();


        builder.Services
            .AddPersistence(builder.Configuration)
            .AddCache(builder.Configuration);

        builder.Services
            .AddValidatorsFromAssemblyContaining<CreateTransactionValidator>()
            .RegisterHandlersFromAssemblyContaining(typeof(CreateTransactionHandler));

        builder.Services.AddMassTransitDefaults(configure =>
        {
            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.AddRabbitMqHost(context);
            });
        });

        return builder;
    }

    private static IServiceCollection ConfigureEndpoints(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "CashFlow Api",
                Description = "Uma Web API que gerencia transações financeiras das suas lojas",
                Contact = new OpenApiContact
                {
                    Name = "Gustavo Souza Silva",
                    Url = new Uri("https://www.linkedin.com/in/gustavo-souza-silva")
                }
            });
            options.SchemaFilter<EnumSchemaFilter>();
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT desta maneira: Bearer {seu token}"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
