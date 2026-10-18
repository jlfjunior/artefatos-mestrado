using CashFlow.Contracts;
using CashFlow.Application.Exceptions;
using CashFlow.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CashFlow.Api.Exceptions;

/// <summary>
/// Converte exceções de domínio/aplicação em respostas HTTP padronizadas.
/// </summary>
public sealed class ApiExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Tenta mapear a exceção para uma resposta HTTP com corpo <see cref="ErrorResponse"/>.
    /// </summary>
    /// <param name="httpContext">Contexto HTTP da requisição.</param>
    /// <param name="exception">Exceção capturada pelo pipeline.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se a exceção foi tratada; false para delegar a outros handlers.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ForbiddenException forbidden:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status403Forbidden,
                    forbidden.Error,
                    forbidden.Message,
                    cancellationToken);

            case ValidationException validation:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    validation.Error,
                    validation.Message,
                    cancellationToken);

            case CodedException coded when coded.Error is "invalid_grant":
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status401Unauthorized,
                    coded.Error,
                    coded.Message,
                    cancellationToken);

            case CodedException coded:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    coded.Error,
                    coded.Message,
                    cancellationToken);

            case ArgumentException argument:
                return await WriteErrorAsync(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    MapValidationCode(argument),
                    argument.Message,
                    cancellationToken);

            default:
                return false;
        }
    }

    private static string MapValidationCode(ArgumentException exception) =>
        exception.ParamName switch
        {
            "description" => "invalid_description",
            "amount" => "invalid_amount",
            "userId" or "user_id" => "invalid_user_id",
            "createdBy" or "created_by" => "invalid_created_by",
            "entryType" or "value" => "invalid_entry_type",
            _ => "invalid_argument"
        };

    private static async Task<bool> WriteErrorAsync(
        HttpContext httpContext,
        int statusCode,
        string error,
        string description,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(error, description), cancellationToken);
        return true;
    }
}
