using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CashFlow.Lancamentos.Api.Infrastructure;
using CashFlow.Shared;

namespace CashFlow.Lancamentos.Api.Application;

public sealed class RegisterEntryHandler
{
    private readonly IEntryCommandStore _commandStore;
    private readonly TimeProvider _timeProvider;

    public RegisterEntryHandler(IEntryCommandStore commandStore, TimeProvider timeProvider)
    {
        _commandStore = commandStore;
        _timeProvider = timeProvider;
    }

    public async Task<RegisterEntryCommandResult> HandleAsync(
        RegisterEntryRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request, idempotencyKey);
        if (validationError is not null)
        {
            return new RegisterEntryCommandResult(RegisterEntryStatus.ErroValidacao, null, validationError);
        }

        request = request with
        {
            MerchantId = request.MerchantId.Trim(),
            Description = request.Description.Trim(),
            Source = string.IsNullOrWhiteSpace(request.Source) ? "API" : request.Source.Trim()
        };

        var requestHash = ComputeRequestHash(request);
        var existing = await _commandStore.FindByIdempotencyAsync(request.MerchantId, idempotencyKey, cancellationToken);

        if (existing is not null)
        {
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return new RegisterEntryCommandResult(
                    RegisterEntryStatus.Conflito,
                    null,
                    "A chave de idempotência já foi utilizada com outro payload.");
            }

            return new RegisterEntryCommandResult(
                RegisterEntryStatus.ReenvioIdempotente,
                MapResponse(existing.Entry, "REENVIADO"),
                null);
        }

        var now = _timeProvider.GetUtcNow();
        var entry = new LedgerEntry(
            Guid.NewGuid(),
            request.MerchantId,
            request.BusinessDate,
            request.Type,
            request.Amount,
            request.Description,
            request.Source,
            idempotencyKey.Trim(),
            now);

        var integrationEvent = new EntryRegisteredIntegrationEvent(
            Guid.NewGuid(),
            entry.EntryId,
            entry.MerchantId,
            entry.BusinessDate,
            entry.Type,
            entry.Amount,
            entry.Description,
            entry.Source,
            now);

        await _commandStore.SaveAsync(entry, requestHash, integrationEvent, cancellationToken);

        return new RegisterEntryCommandResult(
            RegisterEntryStatus.Criado,
            MapResponse(entry, "CRIADO"),
            null);
    }

    private static string? Validate(RegisterEntryRequest request, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return "O cabeçalho Chave-Idempotencia é obrigatório.";
        }

        if (string.IsNullOrWhiteSpace(request.MerchantId))
        {
            return "comercianteId é obrigatório.";
        }

        if (request.Amount <= 0)
        {
            return "valor deve ser maior que zero.";
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return "descricao é obrigatória.";
        }

        return null;
    }

    private static string ComputeRequestHash(RegisterEntryRequest request)
    {
        var payload = string.Join(
            "|",
            request.MerchantId.Trim(),
            request.BusinessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            request.Type,
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.Description.Trim(),
            (request.Source ?? "API").Trim());

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    private static RegisterEntryResponse MapResponse(LedgerEntry entry, string status)
        => new(
            entry.EntryId,
            entry.MerchantId,
            entry.BusinessDate,
            entry.Type,
            entry.Amount,
            entry.Description,
            entry.Source,
            entry.CreatedAt,
            status);
}
