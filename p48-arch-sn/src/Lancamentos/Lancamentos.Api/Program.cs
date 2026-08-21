using FluentValidation;
using Lancamentos.Api.Auth;
using Lancamentos.Api.Endpoints;
using Lancamentos.Application;
using Lancamentos.Domain;
using Lancamentos.Infrastructure;
using Lancamentos.Infrastructure.Persistence;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Log estruturado desde o boot.
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

const string serviceName = "lancamentos-api";
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

builder.Services.AddLancamentosInfrastructure(builder.Configuration);
builder.Services.AddScoped<RegistrarLancamentoService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrarLancamentoRequestValidator>();

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

// Login: valida credenciais e emite um JWT com o papel do usuário. Em produção
// a emissão ficaria num Identity Provider à parte; aqui é simples de propósito.
app.MapPost("/token", (LoginRequest req, IConfiguration config) =>
{
    var usuario = UsuarioStore.Autenticar(req.Usuario, req.Senha);
    if (usuario is null)
        return Results.Unauthorized();

    var token = JwtConfig.EmitirToken(config, usuario.Nome, usuario.Papel);
    return Results.Ok(new { token, papel = usuario.Papel, expiraEmHoras = 8 });
});

app.MapPost("/lancamentos", async (
    RegistrarLancamentoRequest req,
    IValidator<RegistrarLancamentoRequest> validator,
    RegistrarLancamentoService service,
    CancellationToken ct) =>
{
    var validacao = await validator.ValidateAsync(req, ct);
    if (!validacao.IsValid)
        return Results.ValidationProblem(validacao.ToDictionary());

    var tipo = Enum.Parse<TipoLancamento>(req.Tipo, ignoreCase: true);
    var comando = new RegistrarLancamentoCommand(tipo, req.Valor, req.Data, req.Descricao);

    try
    {
        var id = await service.ExecutarAsync(comando, ct);
        return Results.Created($"/lancamentos/{id}", new { id });
    }
    catch (DomainException ex)
    {
        // Rede de segurança: a validação da borda já barra a maioria, mas o
        // domínio é a fonte da verdade das invariantes.
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
    }
})
.RequireAuthorization(p => p.RequireRole(Papeis.Operador, Papeis.Admin))
.WithName("RegistrarLancamento");

// Cria o schema no boot (EnsureCreated). Conveniência de desenvolvimento.
await app.Services.InicializarBancoLancamentosAsync();

app.Run();

public record LoginRequest(string Usuario, string Senha);

// Exposto para os testes de integração.
public partial class Program { }
