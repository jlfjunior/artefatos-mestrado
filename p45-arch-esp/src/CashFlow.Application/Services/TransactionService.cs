using CashFlow.Application.Abstractions;
using CashFlow.Contracts;
using CashFlow.Application.Exceptions;
using CashFlow.Domain.Entities;
using CashFlow.Shared.Enums;
using CashFlow.Shared.Extensions;
using CashFlow.Shared.Messaging;

namespace CashFlow.Application.Services;

/// <summary>
/// Casos de uso de escrita e leitura de lançamentos (imutáveis após criação).
/// </summary>
/// <param name="repository">Repositório de lançamentos.</param>
/// <param name="users">Repositório de usuários.</param>
/// <param name="publisher">Publicador de mensagens.</param>
/// <param name="unitOfWork">Unidade de trabalho.</param>
/// <param name="currentUser">Contexto de usuário atual.</param>
public sealed class TransactionService(
    ITransactionRepository repository,
    IUserRepository users,
    IMessagePublisher publisher,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser)
{
    /// <summary>
    /// Cria lançamento e publica Event Message após persistência.
    /// </summary>
    /// <param name="request">Request de criação de lançamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta de lançamento.</returns>
    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsEmployee)
        {
            throw new ForbiddenException("employee_required", "Somente funcionários podem registrar lançamentos.");
        }

        var owner = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (owner is null || owner.Role != UserRole.Client)
        {
            throw new ValidationException("invalid_user_id", "Usuário cliente inválido.");
        }

        var entryType = ParseEntryType(request.EntryType);
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var transaction = Transaction.Create(
            request.Description,
            request.Amount,
            entryType,
            transactionDate,
            request.UserId,
            currentUser.UserId);

        await unitOfWork.ExecuteAsync(async ct =>
        {
            await repository.InsertAsync(transaction, ct);
            await publisher.PublishAsync(
                TransactionCreatedMessage.Create(
                    transaction.Id,
                    transaction.UserId,
                    transaction.CreatedBy,
                    transaction.Description,
                    transaction.Amount,
                    transaction.EntryType,
                    transaction.TransactionDate),
                ct);
        }, cancellationToken);

        return Map(transaction);
    }

    /// <summary>
    /// Lista lançamentos opcionalmente filtrados por intervalo de datas.
    /// </summary>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de lançamentos.</returns>
    public async Task<IReadOnlyList<TransactionResponse>> ListAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var userFilter = ResolveUserFilter();
        var items = await repository.ListAsync(from, to, userFilter, cancellationToken);
        return items.ToReadOnlyList(Map);
    }

    /// <summary>
    /// Obtém lançamento por identificador.
    /// </summary>
    /// <param name="id">ID do lançamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lançamento ou null se não encontrado.</returns>
    public async Task<TransactionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userFilter = ResolveUserFilter();
        var item = await repository.GetByIdAsync(id, userFilter, cancellationToken);
        return item is null ? null : Map(item);
    }

    private Guid? ResolveUserFilter() =>
        currentUser.IsClient ? currentUser.UserId : null;

    private static EntryType ParseEntryType(string value) =>
        Enum.TryParse<EntryType>(value, true, out var parsed)
            ? parsed
            : throw new ValidationException("invalid_entry_type", $"Tipo de lançamento inválido: {value}.");

    private static TransactionResponse Map(Transaction transaction) => new(
        transaction.Id,
        transaction.UserId,
        transaction.Description,
        transaction.Amount,
        transaction.EntryType.ToString(),
        transaction.TransactionDate,
        transaction.CreatedAt,
        transaction.CreatedBy);
}
