namespace CashFlow.Infrastructure.Configuration;

/// <summary>
/// Configurações de conexão com PostgreSQL.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>Nome da seção de configuração no arquivo de configuração.</summary>
    public const string SectionName = "Database";

    /// <summary>String de conexão com o banco de dados.</
    public string ConnectionString { get; set; } = string.Empty;
}
