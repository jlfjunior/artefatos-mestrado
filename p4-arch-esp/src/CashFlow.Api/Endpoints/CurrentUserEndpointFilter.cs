namespace CashFlow.Api.Endpoints;

public class RequireUserIdFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.GetUserIdAsNullableGuid() is null)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        return await next(context);
    }
}
