using Consolidado.Api.Auth;
using Consolidado.Application;
using Consolidado.Infrastructure;
using Consolidado.Infrastructure.Persistence;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

const string serviceName = "consolidado-api";
var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation()
         .AddEntityFrameworkCoreInstrumentation()
         .AddSource("MassTransit");
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    })
    .WithMetrics(m =>
    {
        m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            m.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    });

builder.Services.AddConsolidadoInfrastructure(builder.Configuration);
builder.Services.AddScoped<AtualizarSaldoService>();
builder.Services.AddScoped<ConsultarSaldoService>();
builder.Services.AddScoped<ConsultarRelatorioService>();

builder.Services.AddJwt(builder.Configuration);

const string corsPolicy = "frontend";
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(o => o.AddPolicy(corsPolicy, p => p
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var rabbitConn = builder.Configuration.GetSection("RabbitMq");
var rabbitUri = new Uri($"amqp://{rabbitConn["Username"] ?? "guest"}:{rabbitConn["Password"] ?? "guest"}@{rabbitConn["Host"] ?? "localhost"}:5672/");

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis")
    .AddRabbitMQ(rabbitConnectionString: rabbitUri, name: "rabbitmq");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// Relatório do saldo diário consolidado num período. O segmento literal
// "relatorio" tem precedência sobre a rota "/consolidado/{data}" no roteamento.
app.MapGet("/consolidado/relatorio", async (
    string de,
    string ate,
    ConsultarRelatorioService service,
    CancellationToken ct) =>
{
    if (!DateOnly.TryParse(de, out var dataDe) || !DateOnly.TryParse(ate, out var dataAte))
        return Results.BadRequest(new { erro = "Datas inválidas. Use o formato yyyy-MM-dd em 'de' e 'ate'." });

    if (dataAte < dataDe)
        return Results.BadRequest(new { erro = "A data final não pode ser anterior à inicial." });

    if (dataAte.DayNumber - dataDe.DayNumber > 366)
        return Results.BadRequest(new { erro = "O período não pode exceder 366 dias." });

    var relatorio = await service.GerarAsync(dataDe, dataAte, ct);
    return Results.Ok(relatorio);
})
.RequireAuthorization(p => p.RequireRole(Papeis.Gerente, Papeis.Admin))
.WithName("RelatorioConsolidado");

app.MapGet("/consolidado/{data}", async (
    string data,
    ConsultarSaldoService service,
    CancellationToken ct) =>
{
    if (!DateOnly.TryParse(data, out var dia))
        return Results.BadRequest(new { erro = "Data inválida. Use o formato yyyy-MM-dd." });

    var saldo = await service.ConsultarAsync(dia, ct);
    return saldo is null
        ? Results.Ok(new { data = dia, saldo = 0m, totalCreditos = 0m, totalDebitos = 0m, mensagem = "Sem lançamentos para o dia." })
        : Results.Ok(saldo);
})
.RequireAuthorization(p => p.RequireRole(Papeis.Gerente, Papeis.Admin))
.WithName("ConsultarSaldoConsolidado");

await app.Services.InicializarBancoConsolidadoAsync();

app.Run();

// Exposto para os testes de integração.
public partial class Program { }
