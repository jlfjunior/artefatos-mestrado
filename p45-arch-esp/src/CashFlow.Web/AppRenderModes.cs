using Microsoft.AspNetCore.Components.Web;

namespace CashFlow.Web;

/// <summary>
/// Modos de renderização compartilhados — evita <c>new</c> inline no Razor (bug de ambiguidade do compilador).
/// </summary>
public static class AppRenderModes
{
    /// <summary>Interatividade server-side sem prerender (sessão/JS disponíveis no primeiro paint interativo).</summary>
    public static readonly InteractiveServerRenderMode Interactive = new(prerender: false);
}
