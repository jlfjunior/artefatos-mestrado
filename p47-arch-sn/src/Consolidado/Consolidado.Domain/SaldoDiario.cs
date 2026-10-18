namespace Consolidado.Domain;

/// <summary>
/// Projeção do saldo de um dia. É o read model: acumula créditos e débitos
/// conforme os lançamentos chegam. A regra de soma vive aqui para não vazar
/// para a camada de infra.
/// </summary>
public class SaldoDiario
{
    public DateOnly Data { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public decimal Saldo { get; private set; }
    public DateTime AtualizadoEmUtc { get; private set; }

    private SaldoDiario() { }

    public SaldoDiario(DateOnly data)
    {
        Data = data;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void AplicarCredito(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Crédito deve ser positivo.", nameof(valor));
        TotalCreditos += valor;
        Recalcular();
    }

    public void AplicarDebito(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Débito deve ser positivo.", nameof(valor));
        TotalDebitos += valor;
        Recalcular();
    }

    private void Recalcular()
    {
        Saldo = TotalCreditos - TotalDebitos;
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}
