using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CashFlow.Api.OpenApi;

/// <summary>
/// Aplica o requisito de segurança Bearer nas operações protegidas por <see cref="AuthorizeAttribute"/>.
/// </summary>
internal sealed class BearerAuthOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            return Task.CompletedTask;
        }

        if (!metadata.OfType<IAuthorizeData>().Any())
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });

        return Task.CompletedTask;
    }
}
