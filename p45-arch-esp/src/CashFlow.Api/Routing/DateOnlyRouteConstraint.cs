using System.Globalization;

namespace CashFlow.Api.Routing;

/// <summary>
/// Restringe parâmetros de rota ao formato ISO <c>yyyy-MM-dd</c> (<see cref="DateOnly"/>).
/// </summary>
public sealed class DateOnlyRouteConstraint : IRouteConstraint
{
    /// <inheritdoc />
    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var raw) || raw is null)
        {
            return false;
        }

        return DateOnly.TryParse(
            raw.ToString(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }
}
