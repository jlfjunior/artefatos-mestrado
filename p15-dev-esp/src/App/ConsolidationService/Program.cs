using Application.Consolidation.Consolidation.Command.CreateConsolidation;
using MassTransit;
using MongoDB.Driver;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Infrastructure.Persistence.Data;
using Infrastructure.Repositories.DependencyInjection;
using Infrastructure.Services.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var mongoSettings = new MongoDbSettings();

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile("/app/consolidationservice/appsettings.json", optional: false, reloadOnChange: true);

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5003);
    });

    var connectionString = Environment.GetEnvironmentVariable("MongoDB__ConnectionString");
    var databaseName = Environment.GetEnvironmentVariable("MongoDB__DatabaseName");

    if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(databaseName))
    {
        mongoSettings.ConnectionString = connectionString;
        mongoSettings.DatabaseName = databaseName;
    }
}
else
{
    mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
}

if (mongoSettings == null)
    throw new ArgumentNullException(nameof(mongoSettings));

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.DatabaseName);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] { }
        }
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateConsolidationCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<CreateConsolidationCommandValidator>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, rollOnFileSizeLimit: false, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddConsolidationRepositories();
builder.Services.AddConsolidationServices();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DailyConsolidationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint("daily_cash_consolidation_queue", e =>
        {
            e.ConfigureConsumer<DailyConsolidationConsumer>(context);
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

app.Run();