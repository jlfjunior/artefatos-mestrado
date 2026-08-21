using CashFlow.Infrastructure.Security;
using CashFlow.Shared.Authorization;
using CashFlow.Shared.Enums;
using FluentMigrator;

namespace CashFlow.Infrastructure.Persistence.Migrations;

/// <summary>
/// Autenticação OAuth2, autorização por referências de acesso e vínculo de lançamentos a clientes.
/// </summary>
[Migration(202606130002)]
public sealed class Migration_20260613_002_AuthAndClientOwnership : Migration
{
    /// <summary>
    /// Cria schema de autenticação, autorização e vínculo multi-cliente.
    /// </summary>
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("full_name").AsString(200).NotNullable()
            .WithColumn("email").AsString(320).NotNullable().Unique()
            .WithColumn("password_hash").AsString(500).NotNullable()
            .WithColumn("role").AsString(20).NotNullable()
            .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        Create.Table("authorization_scopes")
            .WithColumn("code").AsString(100).PrimaryKey()
            .WithColumn("description").AsString(500).NotNullable();

        Create.Table("user_authorization_scopes")
            .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("users", "id")
            .WithColumn("scope_code").AsString(100).NotNullable().ForeignKey("authorization_scopes", "code");

        Create.PrimaryKey("pk_user_authorization_scopes")
            .OnTable("user_authorization_scopes")
            .Columns("user_id", "scope_code");

        Create.Table("oauth_clients")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("client_id").AsString(100).NotNullable().Unique()
            .WithColumn("client_type").AsString(20).NotNullable()
            .WithColumn("grant_types").AsString(500).NotNullable()
            .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        Create.Table("oauth_client_secrets")
            .WithColumn("oauth_client_id").AsGuid().PrimaryKey().ForeignKey("oauth_clients", "id")
            .WithColumn("secret_hash").AsString(500).NotNullable();

        Create.Table("signing_keys")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("key_id").AsString(50).NotNullable().Unique()
            .WithColumn("algorithm").AsString(10).NotNullable()
            .WithColumn("key_use").AsString(10).NotNullable()
            .WithColumn("public_modulus_n").AsString(2048).NotNullable()
            .WithColumn("public_exponent_e").AsString(32).NotNullable()
            .WithColumn("encrypted_private_key").AsString(int.MaxValue).NotNullable()
            .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        InsertScopes();
        InsertUsers();
        InsertOAuthClient();

        Alter.Table("transactions")
            .AddColumn("user_id").AsGuid().Nullable().ForeignKey("users", "id");

        Execute.Sql($"""
            UPDATE transactions
            SET user_id = '{SeedIdentifiers.ClientUserId}'
            WHERE user_id IS NULL
            """);

        Alter.Column("user_id").OnTable("transactions").AsGuid().NotNullable();

        Alter.Table("transactions")
            .AddColumn("created_by").AsGuid().Nullable().ForeignKey("users", "id");

        Execute.Sql($"""
            UPDATE transactions
            SET created_by = '{SeedIdentifiers.EmployeeUserId}'
            WHERE created_by IS NULL
            """);

        Alter.Column("created_by").OnTable("transactions").AsGuid().NotNullable();

        Create.Index("idx_transactions_user_date")
            .OnTable("transactions")
            .OnColumn("user_id").Ascending()
            .OnColumn("transaction_date").Ascending();

        Delete.PrimaryKey("PK_daily_balances").FromTable("daily_balances");

        Alter.Table("daily_balances")
            .AddColumn("user_id").AsGuid().Nullable().ForeignKey("users", "id");

        Execute.Sql($"""
            UPDATE daily_balances
            SET user_id = '{SeedIdentifiers.ClientUserId}'
            WHERE user_id IS NULL
            """);

        Alter.Column("user_id").OnTable("daily_balances").AsGuid().NotNullable();

        Create.PrimaryKey("pk_daily_balances_user_date")
            .OnTable("daily_balances")
            .Columns("user_id", "balance_date");
    }

    /// <summary>
    /// Reverte schema de autenticação, autorização e vínculo multi-cliente.
    /// </summary>
    public override void Down()
    {
        Delete.Table("signing_keys");
        Delete.Table("oauth_client_secrets");
        Delete.Table("oauth_clients");
        Delete.Table("user_authorization_scopes");
        Delete.Table("authorization_scopes");
        Delete.Column("user_id").FromTable("transactions");
        Delete.Column("created_by").FromTable("transactions");
        Delete.PrimaryKey("pk_daily_balances_user_date").FromTable("daily_balances");
        Delete.Column("user_id").FromTable("daily_balances");
        Create.PrimaryKey("PK_daily_balances").OnTable("daily_balances").Column("balance_date");
        Delete.Table("users");
    }

    private void InsertScopes()
    {
        foreach (var (code, description) in ScopeDefinitions())
        {
            Insert.IntoTable("authorization_scopes").Row(new { code, description });
        }
    }

    private void InsertUsers()
    {
        var now = DateTimeOffset.UtcNow;
        var clientHash = PasswordHasher.Hash(SeedIdentifiers.DefaultClientPassword);
        var employeeHash = PasswordHasher.Hash(SeedIdentifiers.DefaultEmployeePassword);

        Insert.IntoTable("users").Row(new
        {
            id = SeedIdentifiers.ClientUserId,
            full_name = "Cliente Demo",
            email = SeedIdentifiers.ClientEmail,
            password_hash = clientHash,
            role = UserRole.Client.ToString(),
            enabled = true,
            created_at = now
        });

        Insert.IntoTable("users").Row(new
        {
            id = SeedIdentifiers.EmployeeUserId,
            full_name = "Funcionário Demo",
            email = SeedIdentifiers.EmployeeEmail,
            password_hash = employeeHash,
            role = UserRole.Employee.ToString(),
            enabled = true,
            created_at = now
        });

        foreach (var scope in AuthorizationScopes.ClientDefaults)
        {
            Insert.IntoTable("user_authorization_scopes").Row(new
            {
                user_id = SeedIdentifiers.ClientUserId,
                scope_code = scope
            });
        }

        foreach (var scope in AuthorizationScopes.EmployeeDefaults)
        {
            Insert.IntoTable("user_authorization_scopes").Row(new
            {
                user_id = SeedIdentifiers.EmployeeUserId,
                scope_code = scope
            });
        }
    }

    private void InsertOAuthClient()
    {
        var now = DateTimeOffset.UtcNow;

        Insert.IntoTable("oauth_clients").Row(new
        {
            id = SeedIdentifiers.OAuthClientInternalId,
            client_id = SeedIdentifiers.OAuthPublicClientId,
            client_type = "confidential",
            grant_types = "password",
            enabled = true,
            created_at = now
        });

        Insert.IntoTable("oauth_client_secrets").Row(new
        {
            oauth_client_id = SeedIdentifiers.OAuthClientInternalId,
            secret_hash = PasswordHasher.Hash(SeedIdentifiers.OAuthClientSecretPlain)
        });
    }

    private static IEnumerable<(string Code, string Description)> ScopeDefinitions() =>
    [
        (AuthorizationScopes.UsersRead, "Listar usuários clientes"),
        (AuthorizationScopes.UsersReadSelf, "Consultar o próprio perfil de cliente"),
        (AuthorizationScopes.TransactionsWrite, "Registrar lançamentos para clientes"),
        (AuthorizationScopes.TransactionsReadAll, "Listar lançamentos de todos os clientes"),
        (AuthorizationScopes.TransactionsReadSelf, "Listar lançamentos do próprio cliente"),
        (AuthorizationScopes.BalancesReadAll, "Consultar saldo consolidado de clientes"),
        (AuthorizationScopes.BalancesReadSelf, "Consultar saldo consolidado do próprio cliente")
    ];
}
