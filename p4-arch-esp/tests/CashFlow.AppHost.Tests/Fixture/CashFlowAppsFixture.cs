using Aspire.Hosting;
using Microsoft.Extensions.Logging;

namespace CashFlow.AppHost.EndToEndTests.Fixture;

[CollectionDefinition("CashFlowApp")]
public class CashFlowAppsFixtureCollection : ICollectionFixture<CashFlowAppsFixture>
{
}

public class CashFlowAppsFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private IDistributedApplicationTestingBuilder _appBuilder = null!;
    private DistributedApplication? _app;

    public async Task InitializeAsync()
    {
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        _appBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CashFlow_AppHost>(cancellationToken);
        _appBuilder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(_appBuilder.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        _appBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await _appBuilder.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
    }

    public HttpClient ClientIdentityServerApi() => _app.CreateHttpClient("cash-flow-identifyserver-api");
    public HttpClient ClientCashFlowApi() => _app.CreateHttpClient("cash-flow-api");

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
    }
}
