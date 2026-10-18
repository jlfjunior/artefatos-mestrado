namespace CashFlow.Shared.Authorization;

/// <summary>
/// Catálogo de referências de acesso atribuídas aos usuários.
/// </summary>
public static class AuthorizationScopes
{
    /// <summary>Listar usuários clientes.</summary>
    public const string UsersRead = "users.read";

    /// <summary>Consultar o próprio perfil de cliente.</summary>
    public const string UsersReadSelf = "users.read.self";

    /// <summary>Registrar lançamentos.</summary>
    public const string TransactionsWrite = "transactions.write";

    /// <summary>Listar lançamentos de todos os clientes.</summary>
    public const string TransactionsReadAll = "transactions.read.all";

    /// <summary>Listar lançamentos do próprio cliente.</summary>
    public const string TransactionsReadSelf = "transactions.read.self";

    /// <summary>Consultar saldo consolidado de clientes.</summary>
    public const string BalancesReadAll = "balances.read.all";

    /// <summary>Consultar saldo consolidado do próprio cliente.</summary>
    public const string BalancesReadSelf = "balances.read.self";

    /// <summary>Escopos padrão atribuídos a funcionários.</summary>
    public static IReadOnlyList<string> EmployeeDefaults { get; } =
    [
        UsersRead,
        TransactionsWrite,
        TransactionsReadAll,
        BalancesReadAll
    ];

    /// <summary>Escopos padrão atribuídos a clientes.</summary>
    public static IReadOnlyList<string> ClientDefaults { get; } =
    [
        UsersReadSelf,
        TransactionsReadSelf,
        BalancesReadSelf
    ];

    /// <summary>
    /// Escopos expostos no documento OpenID Connect.
    /// </summary>
    public static IReadOnlyList<string> Supported { get; } =
    [
        UsersRead,
        UsersReadSelf,
        TransactionsWrite,
        TransactionsReadAll,
        TransactionsReadSelf,
        BalancesReadAll,
        BalancesReadSelf
    ];
}
