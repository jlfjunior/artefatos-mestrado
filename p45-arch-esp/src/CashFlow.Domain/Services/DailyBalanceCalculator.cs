using CashFlow.Shared.Enums;

namespace CashFlow.Domain.Services;

/// <summary>
/// Serviço de domínio para recalcular totais do consolidado diário.
/// </summary>
public static class DailyBalanceCalculator
{
    /// <summary>
    /// Aplica delta de crédito/débito sobre totais existentes.
    /// </summary>
    /// <param name="currentCredits">Total de créditos antes do delta.</param>
    /// <param name="currentDebits">Total de débitos antes do delta.</param>
    /// <param name="creditDelta">Incremento de crédito a aplicar.</param>
    /// <param name="debitDelta">Incremento de débito a aplicar.</param>
    /// <returns>Totais recalculados após aplicar os deltas.</returns>
    public static (decimal TotalCredits, decimal TotalDebits, decimal Balance) ApplyDelta(
        decimal currentCredits,
        decimal currentDebits,
        decimal creditDelta,
        decimal debitDelta)
    {
        var totalCredits = currentCredits + creditDelta;
        var totalDebits = currentDebits + debitDelta;
        var balance = totalCredits - totalDebits;

        return (totalCredits, totalDebits, balance);
    }

    /// <summary>
    /// Calcula o saldo acumulado ao fim do dia a partir do saldo anterior e dos totais do dia.
    /// </summary>
    /// <param name="previousBalance">Saldo acumulado do último dia com movimentação anterior.</param>
    /// <param name="dayCredits">Total de créditos do dia.</param>
    /// <param name="dayDebits">Total de débitos do dia.</param>
    /// <returns>Saldo acumulado ao fim do dia contábil.</returns>
    public static decimal ComputeCumulativeBalance(
        decimal previousBalance,
        decimal dayCredits,
        decimal dayDebits) =>
        previousBalance + dayCredits - dayDebits;

    /// <summary>
    /// Calcula deltas ao adicionar um lançamento (create).
    /// </summary>
    /// <param name="amount">Valor absoluto do lançamento.</param>
    /// <param name="entryType">Tipo do lançamento.</param>
    /// <returns>Deltas de crédito e débito correspondentes ao lançamento.</returns>
    public static (decimal CreditDelta, decimal DebitDelta) ComputeAdditionDelta(
        decimal amount,
        EntryType entryType)
    {
        var creditDelta = entryType == EntryType.Credit ? amount : 0;
        var debitDelta = entryType == EntryType.Debit ? amount : 0;
        return (creditDelta, debitDelta);
    }
}
