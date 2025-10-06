using Flow.Application;
using Flow.Application.MessageBrokers;
using Flow.Core.Security.Tokens;
using Flow.Infra;
using Flow.Infra.Data;
using Flow.Infra.Migrations;
using Flow.Infra.Tokens;
using Flow.Services.Transactions.Filters;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjectionTransactionInfra(builder.Configuration);
builder.Services.AddDependencyInjectionTransactionApplication(builder.Configuration);

builder.Services.AddScoped<AuthenticatedUserFilter>();
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddFluentMigratorCore().ConfigureRunner(rb =>
{
    rb.AddSqlServer()
      .AddAllDatabases()
      .WithGlobalConnectionString(builder.Configuration.GetConnectionString("RelationalDb"))
      .ScanIn(Assembly.Load("Flow.Infra"))
      .For.All();
});

builder.Services.Configure<SelectingProcessorAccessorOptions>(options =>
{
    options.ProcessorId = "sqlserver";
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

var app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

UpdateDatabase();

app.Run();

void UpdateDatabase()
{
    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    using var context = serviceScope.ServiceProvider.GetService<RelationalDbContext>();

    var connection = builder.Configuration.GetConnectionString("DefaultConnection");

    Database.CreateDatabase(connection);
    app.DatabaseMigrate();
}
