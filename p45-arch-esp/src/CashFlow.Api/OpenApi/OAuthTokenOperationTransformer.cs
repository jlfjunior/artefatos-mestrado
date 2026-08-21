using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CashFlow.Api.OpenApi;

/// <summary>
/// Documenta o corpo de <c>POST /oauth/token</c> nos formatos JSON e form-urlencoded.
/// </summary>
internal sealed class OAuthTokenOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(context.Description.RelativePath, "oauth/token", StringComparison.OrdinalIgnoreCase)
            || !HttpMethods.IsPost(context.Description.HttpMethod!))
        {
            return Task.CompletedTask;
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Description =
                "Credenciais OAuth2 (grant_type=password). " +
                "Use JSON no Scalar ou form-urlencoded conforme RFC 6749.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchemaReference("OAuthTokenRequest", context.Document)
                },
                ["application/x-www-form-urlencoded"] = new OpenApiMediaType
                {
                    Schema = CreateFormSchema()
                }
            }
        };

        return Task.CompletedTask;
    }

    private static OpenApiSchema CreateFormSchema() => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "grant_type", "username", "password" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["grant_type"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Grant type OAuth2. Valor suportado: password.",
                Default = "password"
            },
            ["client_id"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Client id público (opcional se enviado via Authorization: Basic)."
            },
            ["client_secret"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Secret do client (opcional se enviado via Authorization: Basic)."
            },
            ["username"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "E-mail do usuário."
            },
            ["password"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Senha do usuário.",
                Format = "password"
            }
        }
    };
}
