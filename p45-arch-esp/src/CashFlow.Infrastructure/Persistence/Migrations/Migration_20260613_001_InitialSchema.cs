using FluentMigrator;

namespace CashFlow.Infrastructure.Persistence.Migrations;

/// <summary>
/// Migration inicial: tabelas de lançamentos e saldo diário.
/// </summary>
[Migration(202606130001)]
public sealed class Migration_20260613_001_InitialSchema : Migration
{
    /// <summary>
    /// Cria tabelas iniciais de lançamentos e saldo diário.
    /// </summary>
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");

        Create.Table("transactions")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("description").AsString(500).NotNullable()
            .WithColumn("amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("entry_type").AsString(10).NotNullable()
            .WithColumn("transaction_date").AsDate().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        Create.Index("idx_transactions_transaction_date")
            .OnTable("transactions")
            .OnColumn("transaction_date");

        Create.Table("daily_balances")
            .WithColumn("balance_date").AsDate().PrimaryKey()
            .WithColumn("total_credits").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("total_debits").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("balance").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("last_event_id").AsGuid().Nullable()
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable();
    }

    /// <summary>
    /// Remove tabelas criadas por esta migration.
    /// </summary>
    public override void Down()
    {
        Delete.Table("daily_balances");
        Delete.Table("transactions");
    }
}
