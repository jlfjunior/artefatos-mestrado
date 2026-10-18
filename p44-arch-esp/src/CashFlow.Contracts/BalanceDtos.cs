namespace CashFlow.Contracts;

/// <summary>
/// Saldo consolidado de um dia contábil.
/// </summary>
/// <param name="UserId">Usuário cliente titular do consolidado.</param>
/// <param name="Date">Data contábil do consolidado.</param>
/// <param name="TotalCredits">Soma dos créditos do dia.</param>
/// <param name="TotalDebits">Soma dos débitos do dia.</param>
/// <param name="Balance">Saldo acumulado ao fim do dia (inclui saldo anterior quando não há movimentação na data).</param>
/// <param name="UpdatedAt">Instante UTC da última atualização da projeção materializada.</param>
public sealed record BalanceResponse(
    Guid UserId,
    DateOnly Date,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    DateTime UpdatedAt);

/// <summary>
/// Resultado paginado de consulta de saldos consolidados.
/// </summary>
/// <param name="Items">Itens da página atual.</param>
/// <param name="Page">Número da página retornada (base 1).</param>
/// <param name="PageSize">Tamanho da página solicitado.</param>
/// <param name="TotalCount">Quantidade total de dias com registro, considerando o filtro aplicado.</param>
public sealed record PaginatedBalanceResponse(
    IReadOnlyList<BalanceResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
