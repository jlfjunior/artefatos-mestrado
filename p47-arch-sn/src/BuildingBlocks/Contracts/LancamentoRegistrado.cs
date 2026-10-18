namespace FluxoCaixa.Contracts;

/// <summary>
/// Evento de integração publicado pelo serviço de Lançamentos sempre que um
/// lançamento é registrado. O Consolidado consome para projetar o saldo do dia.
/// O contrato vive num projeto à parte para que produtor e consumidor não
/// divirjam de schema.
/// </summary>
public record LancamentoRegistrado
{
    /// <summary>Identificador do lançamento — chave de idempotência no consumidor.</summary>
    public Guid LancamentoId { get; init; }

    /// <summary>"Credito" ou "Debito".</summary>
    public string Tipo { get; init; } = string.Empty;

    public decimal Valor { get; init; }

    /// <summary>Data do lançamento (sem hora). Define o dia do consolidado.</summary>
    public DateOnly Data { get; init; }

    public string? Descricao { get; init; }

    /// <summary>Momento em que o lançamento foi registrado, em UTC.</summary>
    public DateTime OcorridoEmUtc { get; init; }
}
