namespace CashFlow.Domain.Entities;

/// <summary>
/// Projeção de leitura com saldo consolidado de um dia.
/// </summary>
public sealed class DailyBalance
{
    /// <summary>Identificador do usuário cliente titular.</summary>
    public Guid UserId { get; init; }

    /// <summary>Data contábil do consolidado.</summary>
    public DateOnly BalanceDate { get; init; }

    /// <summary>Soma dos créditos do dia.</summary>
    public decimal TotalCredits { get; init; }

    /// <summary>Soma dos débitos do dia.</summary>
    public decimal TotalDebits { get; init; }

    /// <summary>Saldo acumulado ao fim do dia (saldo anterior + movimentos do dia).</summary>
    public decimal Balance { get; init; }

    /// <summary>Identificador do último evento aplicado na projeção.</summary>
    public Guid? LastEventId { get; init; }

    /// <summary>Instante UTC da última atualização da projeção.</summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Cria saldo zerado para uma data sem movimentações.
    /// </summary>
    /// <param name="userId">Identificador do usuário cliente titular.</param>
    /// <param name="date">Data contábil do consolidado.</param>
    /// <returns>Projeção com totais zerados.</returns>
    public static DailyBalance Empty(Guid userId, DateOnly date) => new()
    {
        UserId = userId,
        BalanceDate = date,
        TotalCredits = 0,
        TotalDebits = 0,
        Balance = 0,
        UpdatedAt = DateTime.UtcNow
    };
}
