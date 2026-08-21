using CashFlow.Application;
using CashFlow.Application.Authorization;
using CashFlow.Api.Exceptions;
using CashFlow.Api.OpenApi;
using CashFlow.Api.Routing;
using CashFlow.Contracts.Json;
using CashFlow.Infrastructure;
using CashFlow.Infrastructure.Auth;
using CashFlow.Infrastructure.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<DateOnlyRouteConstraint>("date");
});

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options => ContractsJsonSerializerOptions.Apply(options.JsonSerializerOptions));
builder.Services.ConfigureHttpJsonOptions(options =>
    ContractsJsonSerializerOptions.Apply(options.SerializerOptions));
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "CashFlow API",
            Version = "v1",
            Description =
                "API REST para lançamentos imutáveis de fluxo de caixa e consulta de saldo diário consolidado. " +
                "Autenticação OAuth2 com JWT RS256. Obtenha o token em `POST /oauth/token`."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT obtido em POST /oauth/token (grant_type=password)."
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer<BearerAuthOperationTransformer>();
    options.AddOperationTransformer<OAuthTokenOperationTransformer>();
})
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddCashFlowAuthentication(builder.Configuration)
    .AddCashFlowHealthChecks()
    .AddAuthorization(options => AuthorizationPolicies.Register(options))
    .AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("CashFlow API")
            .AddPreferredSecuritySchemes("Bearer")
            .AddHttpAuthentication("Bearer", scheme => scheme.Token = string.Empty);
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/liveness", () => Results.Ok(new { status = "alive", service = "cashflow-api" }))
    .AllowAnonymous();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true
}).AllowAnonymous();

app.Run();
