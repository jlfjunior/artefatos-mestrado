namespace CashFlow.Shared.Authorization;

/// <summary>
/// Identificadores estáveis usados na carga inicial de dados.
/// </summary>
public static class SeedIdentifiers
{
    /// <summary>Identificador do usuário cliente de demonstração.</summary>
    public static readonly Guid ClientUserId = Guid.Parse("40830623-48b3-413f-9319-375501484841");

    /// <summary>Identificador do usuário funcionário de demonstração.</summary>
    public static readonly Guid EmployeeUserId = Guid.Parse("99599789-b79c-4701-8626-151553925384");

    /// <summary>Identificador interno do client OAuth da interface web.</summary>
    public static readonly Guid OAuthClientInternalId = Guid.Parse("57608205-279c-4817-9d09-343234124093");

    /// <summary>E-mail do cliente de demonstração.</summary>
    public const string ClientEmail = "cliente@cashflow.local";

    /// <summary>E-mail do funcionário de demonstração.</summary>
    public const string EmployeeEmail = "funcionario@cashflow.local";

    /// <summary>Client id público do OAuth da interface web.</summary>
    public const string OAuthPublicClientId = "cashflow.web";

    /// <summary>Secret em texto plano do client OAuth da interface web (somente ambiente demo).</summary>
    public const string OAuthClientSecretPlain = "bb222c98-ead0-44cd-b12a-a54a9f6ee1a4";

    /// <summary>Senha inicial do cliente de demonstração.</summary>
    public const string DefaultClientPassword = "Cliente@123";

    /// <summary>Senha inicial do funcionário de demonstração.</summary>
    public const string DefaultEmployeePassword = "Funcionario@123";
}
