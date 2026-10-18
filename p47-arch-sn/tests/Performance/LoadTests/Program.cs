using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace FluxoCaixa.LoadTests;

/// <summary>
/// Teste de carga do read side. O requisito não-funcional do projeto é claro:
/// em pico, o Consolidado recebe 50 req/s com no máximo 5% de perda. Este
/// cenário reproduz exatamente isso — injeta 50 requisições por segundo no
/// endpoint de consulta por um período sustentado e verifica se a taxa de
/// sucesso fica em pelo menos 95%.
///
/// Não faz parte do `dotnet test`: é um executável que rodo com `dotnet run`
/// quando a infra e as APIs já estão no ar (via docker compose). As URLs e as
/// credenciais saem de variáveis de ambiente, com defaults que batem com o
/// docker-compose do repositório.
/// </summary>
public static class Program
{
    public static int Main()
    {
        var config = CarregarConfig();

        Console.WriteLine($"Lançamentos (token): {config.LancamentosBaseUrl}");
        Console.WriteLine($"Consolidado (consulta): {config.ConsolidadoBaseUrl}");
        Console.WriteLine($"Usuário: {config.Usuario} | Dia consultado: {config.Data}");
        Console.WriteLine($"Carga: {config.TaxaPorSegundo} req/s por {config.DuracaoSegundos}s");

        string token;
        try
        {
            token = AutenticarAsync(config).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Falha ao autenticar em {config.LancamentosBaseUrl}/token: {ex.Message}");
            Console.Error.WriteLine("As APIs estão no ar? Suba a stack com `docker compose up` antes de rodar a carga.");
            return 1;
        }

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = $"{config.ConsolidadoBaseUrl}/consolidado/{config.Data}";

        var scenario = Scenario.Create("consolidado_50_rps", async _ =>
            {
                var request = Http.CreateRequest("GET", url)
                    .WithHeader("Accept", "application/json");

                var response = await Http.Send(httpClient, request);
                return response;
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                // O coração do RNF: injeta uma taxa constante de chegada,
                // independente da latência de resposta (modelo de chegada aberto).
                Simulation.Inject(
                    rate: config.TaxaPorSegundo,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromSeconds(config.DuracaoSegundos)));

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("relatorios-carga")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Txt)
            .Run();

        return AvaliarResultado(stats, config);
    }

    /// <summary>
    /// Traduz as estatísticas do NBomber no veredito do RNF. Faço a avaliação no
    /// código (em vez de só confiar no relatório) para que o processo termine com
    /// exit code != 0 quando a meta não é cumprida — assim a carga pode entrar
    /// num pipeline e quebrar o build se o serviço degradar.
    /// </summary>
    private static int AvaliarResultado(NBomber.Contracts.Stats.NodeStats stats, CargaConfig config)
    {
        var cenario = stats.ScenarioStats[0];
        var ok = cenario.Ok.Request.Count;
        var falhas = cenario.Fail.Request.Count;
        var total = ok + falhas;

        if (total == 0)
        {
            Console.Error.WriteLine("Nenhuma requisição foi enviada. Algo na configuração da carga está errado.");
            return 1;
        }

        var taxaFalha = (double)falhas / total * 100.0;
        var taxaSucesso = 100.0 - taxaFalha;

        var lat = cenario.Ok.Latency;

        Console.WriteLine();
        Console.WriteLine("===== Veredito do RNF (50 req/s, <= 5% de perda) =====");
        Console.WriteLine($"Total de requisições : {total}");
        Console.WriteLine($"Sucesso              : {ok} ({taxaSucesso:F2}%)");
        Console.WriteLine($"Falhas               : {falhas} ({taxaFalha:F2}%)");
        Console.WriteLine($"Latência p50         : {lat.Percent50} ms");
        Console.WriteLine($"Latência p95         : {lat.Percent95} ms");
        Console.WriteLine($"Latência p99         : {lat.Percent99} ms");
        Console.WriteLine($"Latência máx         : {lat.MaxMs} ms");
        Console.WriteLine("======================================================");

        if (taxaFalha > config.PerdaMaximaPercentual)
        {
            Console.Error.WriteLine(
                $"REPROVADO: taxa de falha {taxaFalha:F2}% passou do limite de {config.PerdaMaximaPercentual:F2}%.");
            return 1;
        }

        Console.WriteLine(
            $"APROVADO: taxa de falha {taxaFalha:F2}% dentro do limite de {config.PerdaMaximaPercentual:F2}%.");
        return 0;
    }

    private static async Task<string> AutenticarAsync(CargaConfig config)
    {
        using var http = new HttpClient { BaseAddress = new Uri(config.LancamentosBaseUrl) };

        var resposta = await http.PostAsJsonAsync("/token", new
        {
            usuario = config.Usuario,
            senha = config.Senha
        });

        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        var token = corpo.GetProperty("token").GetString();

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("O endpoint /token respondeu sem um token utilizável.");

        return token;
    }

    private static CargaConfig CarregarConfig() => new()
    {
        LancamentosBaseUrl = Env("LOADTEST_LANCAMENTOS_URL", "http://localhost:5080"),
        ConsolidadoBaseUrl = Env("LOADTEST_CONSOLIDADO_URL", "http://localhost:5090"),
        Usuario = Env("LOADTEST_USUARIO", "gerente"),
        Senha = Env("LOADTEST_SENHA", "Senha@123"),
        Data = Env("LOADTEST_DATA", DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")),
        TaxaPorSegundo = EnvInt("LOADTEST_RATE", 50),
        DuracaoSegundos = EnvInt("LOADTEST_DURACAO_SEG", 60),
        PerdaMaximaPercentual = EnvDouble("LOADTEST_PERDA_MAX_PCT", 5.0)
    };

    private static string Env(string nome, string padrao)
        => Environment.GetEnvironmentVariable(nome) is { Length: > 0 } v ? v : padrao;

    private static int EnvInt(string nome, int padrao)
        => int.TryParse(Environment.GetEnvironmentVariable(nome), out var v) ? v : padrao;

    private static double EnvDouble(string nome, double padrao)
        => double.TryParse(Environment.GetEnvironmentVariable(nome),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v) ? v : padrao;

    private sealed class CargaConfig
    {
        public required string LancamentosBaseUrl { get; init; }
        public required string ConsolidadoBaseUrl { get; init; }
        public required string Usuario { get; init; }
        public required string Senha { get; init; }
        public required string Data { get; init; }
        public required int TaxaPorSegundo { get; init; }
        public required int DuracaoSegundos { get; init; }
        public required double PerdaMaximaPercentual { get; init; }
    }
}
