namespace ControleLancamentos.Domain.Lancamentos;

public class ConsolidadoDiario
{
    internal ConsolidadoDiario()
    {
    }

    public DateOnly DataReferencia { get; internal set; }

    public decimal TotalCreditos { get; internal set; }

    public decimal TotalDebitos { get; internal set; }

    public decimal Saldo { get; internal set; }

    public int QuantidadeLancamentos { get; internal set; }
}
