using System.Net.Http.Headers;
using System.Text.Json;
using CashFlow.Contracts;
using CashFlow.Contracts.Json;
using CashFlow.Web.Configuration;
using Microsoft.Extensions.Options;

namespace CashFlow.Web.Services;

/// <summary>
/// Cliente HTTP para a API unificada de fluxo de caixa.
/// </summary>
public sealed class CashFlowApiClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = ContractsJsonSerializerOptions.Default;

    private readonly HttpClient _client;
    private readonly AuthSession _session;

    /// <summary>
    /// Cria uma instância do cliente HTTP para a API de fluxo de caixa.
    /// </summary>
    /// <param name="options">Configurações da API.</param>
    /// <param name="session">Sessão com o token do usuário logado.</param>
    public CashFlowApiClient(IOptions<ApiSettings> options, AuthSession session)
    {
        _session = session;
        _client = new HttpClient
        {
            BaseAddress = new Uri(options.Value.BaseUrl)
        };
    }

    /// <summary>
    /// Obtém a lista de lançamentos da API.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de lançamentos ou null se não encontrado.</returns>
    public Task<IReadOnlyList<TransactionResponse>?> GetTransactionsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<TransactionResponse>>(HttpMethod.Get, "api/transactions", cancellationToken);

    /// <summary>
    /// Cria um novo lançamento na API.
    /// </summary>
    /// <param name="request">Requisição de criação de lançamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lançamento criado ou null se não encontrado.</returns>
    public async Task<TransactionResponse?> CreateTransactionAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Post, "api/transactions");
        message.Content = JsonContent.Create(request, options: JsonOptions);
        var response = await _client.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions, cancellationToken);
    }

    /// <summary>
    /// Obtém a lista de clientes da API.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de clientes ou null se não encontrado.</returns>
    public Task<IReadOnlyList<ClientResponse>?> GetClientsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ClientResponse>>(HttpMethod.Get, "api/clients", cancellationToken);

    /// <summary>
    /// Obtém o cliente atual da API.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Cliente atual ou null se não encontrado.</returns>
    public Task<ClientResponse?> GetCurrentClientAsync(CancellationToken cancellationToken = default) =>
        SendAsync<ClientResponse>(HttpMethod.Get, "api/clients/me", cancellationToken);

    /// <summary>
    /// Obtém o saldo de um cliente em uma data específica.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="date">Data do saldo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Saldo do cliente ou null se não encontrado.</returns>
    public Task<BalanceResponse?> GetBalanceAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        SendAsync<BalanceResponse>(
            HttpMethod.Get,
            $"api/balances/{date:yyyy-MM-dd}?userId={userId:D}",
            cancellationToken);

    /// <summary>
    /// Obtém o saldo de um cliente no dia atual.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Saldo do cliente ou null se não encontrado.</returns>
    public Task<BalanceResponse?> GetTodayBalanceAsync(Guid userId, CancellationToken cancellationToken = default) =>
        SendAsync<BalanceResponse>(HttpMethod.Get, $"api/balances/today?userId={userId:D}", cancellationToken);

    #region IDisposable

    private bool _disposed = false;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _client.Dispose();
        }

        _disposed = true;
    }

    #endregion

    private async Task<T?> SendAsync<T>(HttpMethod method, string url, CancellationToken cancellationToken)
    {
        using var message = CreateRequest(method, url);
        var response = await _client.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var detail = string.IsNullOrWhiteSpace(body)
                ? response.ReasonPhrase
                : body.Length > 200 ? body[..200] : body;
            throw new HttpRequestException(
                $"A API retornou {(int)response.StatusCode} ({response.StatusCode}): {detail}");
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        return request;
    }
}
