namespace ControleLancamentos.Domain.Lancamentos;

public class Lancamento
{
    internal Lancamento()
    {
    }

    public Guid Id { get; internal set; }

    public decimal Valor { get; internal set; }

    public TipoLancamento Tipo { get; internal set; }

    public DateTime DataLancamento { get; internal set; }

    public string? Descricao { get; internal set; }
}
