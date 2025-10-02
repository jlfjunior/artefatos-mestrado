using Application;
using Application.Consumers;
using Infrastructure;
using Infrastructure.Configurations;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace Web.Api;

/// <summary>
/// </summary>
public class Program
{
    /// <summary>
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        builder.Services.AddAuthorization();

        builder.Services
            .AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddAutoMapper(typeof(DailySummaryProfile));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<TransactionCreatedConsumer>();
            x.AddConsumer<TransactionUpdatedConsumer>();
            x.AddConsumer<TransactionDeletedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.ReceiveEndpoint("transaction-created-queue", e =>
                {
                    e.ConfigureConsumer<TransactionCreatedConsumer>(context);
                });

                cfg.ReceiveEndpoint("transaction-updated-queue", e =>
                {
                    e.ConfigureConsumer<TransactionUpdatedConsumer>(context);
                });

                cfg.ReceiveEndpoint("transaction-deleted-queue", e =>
                {
                    e.ConfigureConsumer<TransactionDeletedConsumer>(context);
                });
            });
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["Redis:Host"] ?? "localhost";
            options.InstanceName = "verity_cache";
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Daily Summary API", Version = "v1" });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Insira o token JWT no formato: Bearer {seu token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            c.AddSecurityDefinition("Bearer", securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
