namespace FluxoCaixa.Consolidado.Domain.Entities;

public class ConsolidadoDiario
{
    public DateOnly Data { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public decimal SaldoConsolidado => TotalCreditos - TotalDebitos;
    public int QuantidadeLancamentos { get; private set; }
    public DateTimeOffset UltimaAtualizacao { get; private set; }

    protected ConsolidadoDiario() { }

    public ConsolidadoDiario(DateOnly data)
    {
        Data = data;
        TotalCreditos = 0;
        TotalDebitos = 0;
        QuantidadeLancamentos = 0;
        UltimaAtualizacao = DateTimeOffset.UtcNow;
    }

    public void AplicarLancamento(string tipo, decimal valor)
    {
        if (tipo.ToUpper() == "CREDITO")
            TotalCreditos += valor;
        else
            TotalDebitos += valor;

        QuantidadeLancamentos++;
        UltimaAtualizacao = DateTimeOffset.UtcNow;
    }
}
