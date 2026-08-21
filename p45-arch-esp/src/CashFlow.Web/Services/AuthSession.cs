using System.Text.Json;
using Microsoft.JSInterop;

namespace CashFlow.Web.Services;

/// <summary>
/// Armazena o access token da sessão Blazor, com persistência no sessionStorage do navegador.
/// </summary>
public sealed class AuthSession(IJSRuntime js)
{
    private bool _restored;

    /// <summary>
    /// Token de acesso.
    /// </summary>
    public string? AccessToken { get; private set; }

    /// <summary>
    /// Papel do usuário.
    /// </summary>
    public string? Role { get; private set; }

    /// <summary>
    /// ID do usuário.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Verifica se o usuário está autenticado.
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    /// <summary>
    /// Verifica se o usuário é um funcionário.
    /// </summary>
    public bool IsEmployee => string.Equals(Role, "employee", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Verifica se o usuário é um cliente.
    /// </summary>
    public bool IsClient => string.Equals(Role, "client", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Restaura a sessão do sessionStorage (após refresh ou novo circuito).
    /// </summary>
    public async Task EnsureRestoredAsync()
    {
        if (_restored)
        {
            return;
        }

        _restored = true;

        if (IsAuthenticated)
        {
            return;
        }

        try
        {
            var json = await js.InvokeAsync<string?>("cashFlowAuth.get");
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var stored = JsonSerializer.Deserialize<StoredAuth>(json);
            if (stored is null || string.IsNullOrWhiteSpace(stored.AccessToken))
            {
                return;
            }

            SetToken(stored.AccessToken, stored.Role, stored.UserId);
        }
        catch (JSException)
        {
            // JS indisponível neste momento — segue sem sessão persistida.
        }
    }

    /// <summary>
    /// Define o token de acesso, papel e ID do usuário.
    /// </summary>
    /// <param name="accessToken">Token de acesso.</param>
    /// <param name="role">Papel do usuário.</param>
    /// <param name="userId">ID do usuário.</param>
    public void SetToken(string accessToken, string role, Guid userId)
    {
        AccessToken = accessToken;
        Role = role;
        UserId = userId;
    }

    /// <summary>
    /// Define o token e persiste no sessionStorage.
    /// </summary>
    /// <param name="accessToken">Token de acesso.</param>
    /// <param name="role">Papel do usuário.</param>
    /// <param name="userId">ID do usuário.</param>
    public async Task SetTokenAsync(string accessToken, string role, Guid userId)
    {
        SetToken(accessToken, role, userId);
        var json = JsonSerializer.Serialize(new StoredAuth(accessToken, role, userId));
        await js.InvokeVoidAsync("cashFlowAuth.set", json);
    }

    /// <summary>
    /// Limpa a sessão de autenticação em memória.
    /// </summary>
    public void Clear()
    {
        AccessToken = null;
        Role = null;
        UserId = Guid.Empty;
    }

    /// <summary>
    /// Limpa a sessão em memória e no sessionStorage.
    /// </summary>
    public async Task ClearAsync()
    {
        Clear();

        try
        {
            await js.InvokeVoidAsync("cashFlowAuth.clear");
        }
        catch (JSException)
        {
            // Ignora falha ao limpar storage.
        }
    }

    private sealed record StoredAuth(string AccessToken, string Role, Guid UserId);
}
