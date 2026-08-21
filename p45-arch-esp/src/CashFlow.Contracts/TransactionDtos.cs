namespace CashFlow.Contracts;

/// <summary>
/// Corpo da requisição para registrar um lançamento imutável de fluxo de caixa.
/// A data contábil é atribuída pelo servidor (dia UTC corrente).
/// </summary>
/// <param name="UserId" example="a1111111-1111-1111-1111-111111111111">Usuário cliente titular do lançamento.</param>
/// <param name="Description" example="Venda balcão">Descrição livre do lançamento.</param>
/// <param name="Amount" example="150.00">Valor absoluto do lançamento. Deve ser maior que zero.</param>
/// <param name="EntryType" example="Credit">Tipo do lançamento: <c>Credit</c> (entrada) ou <c>Debit</c> (saída).</param>
public sealed record CreateTransactionRequest(
    Guid UserId,
    string Description,
    decimal Amount,
    string EntryType);

/// <summary>
/// Representação de um lançamento persistido.
/// </summary>
/// <param name="Id">Identificador único do lançamento.</param>
/// <param name="UserId">Usuário cliente titular do lançamento.</param>
/// <param name="Description">Descrição informada na criação.</param>
/// <param name="Amount">Valor absoluto registrado.</param>
/// <param name="EntryType">Tipo do lançamento: <c>Credit</c> ou <c>Debit</c>.</param>
/// <param name="TransactionDate">Data contábil atribuída pelo servidor na criação (dia UTC corrente).</param>
/// <param name="CreatedAt">Instante UTC em que o lançamento foi gravado.</param>
/// <param name="CreatedBy">Usuário que registrou o lançamento.</param>
public sealed record TransactionResponse(
    Guid Id,
    Guid UserId,
    string Description,
    decimal Amount,
    string EntryType,
    DateOnly TransactionDate,
    DateTime CreatedAt,
    Guid CreatedBy);
