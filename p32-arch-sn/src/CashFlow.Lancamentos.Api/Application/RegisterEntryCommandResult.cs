using CashFlow.Shared;

namespace CashFlow.Lancamentos.Api.Application;

public enum RegisterEntryStatus
{
    Criado = 1,
    ReenvioIdempotente = 2,
    Conflito = 3,
    ErroValidacao = 4
}

public sealed record RegisterEntryCommandResult(
    RegisterEntryStatus Status,
    RegisterEntryResponse? Response,
    string? ErrorMessage);
