using FluxoCaixa.Lancamentos.Domain.Exceptions;

namespace FluxoCaixa.Lancamentos.Domain.Entities;

public enum TipoLancamento
{
    Debito = 0,
    Credito = 1
}

public class Lancamento
{
    public Guid Id { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; }
    public DateTimeOffset DataHora { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    // Construtor vazio para ORM
    protected Lancamento() { }

    public DateOnly DataConsolidado(string fusoHorario)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(fusoHorario);
        var local = TimeZoneInfo.ConvertTime(DataHora, tz);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public static Lancamento Criar(TipoLancamento tipo, decimal valor, string descricao, DateTimeOffset dataHora)
    {
        if (valor <= 0) 
            throw new DomainException("Valor deve ser positivo.");
            
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição é obrigatória.");

        return new Lancamento
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Valor = valor,
            Descricao = descricao,
            DataHora = dataHora.ToUniversalTime(),
            CriadoEm = DateTimeOffset.UtcNow
        };
    }
}
