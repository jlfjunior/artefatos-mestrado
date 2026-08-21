namespace Lancamentos.Domain;

/// <summary>
/// Raiz de agregado do write side. Um lançamento é imutável depois de criado —
/// não existe caso de uso de edição no escopo, então a entidade não expõe
/// setters públicos. A fábrica <see cref="Registrar"/> centraliza as invariantes.
/// </summary>
public class Lancamento
{
    public Guid Id { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly Data { get; private set; }
    public string? Descricao { get; private set; }
    public DateTime CriadoEmUtc { get; private set; }

    // EF Core precisa de um construtor sem parâmetros.
    private Lancamento() { }

    private Lancamento(Guid id, TipoLancamento tipo, decimal valor, DateOnly data, string? descricao, DateTime criadoEmUtc)
    {
        Id = id;
        Tipo = tipo;
        Valor = valor;
        Data = data;
        Descricao = descricao;
        CriadoEmUtc = criadoEmUtc;
    }

    public static Lancamento Registrar(TipoLancamento tipo, decimal valor, DateOnly data, string? descricao)
    {
        if (valor <= 0)
            throw new DomainException("O valor do lançamento deve ser maior que zero.");

        if (!Enum.IsDefined(tipo))
            throw new DomainException("Tipo de lançamento inválido.");

        var descricaoLimpa = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        if (descricaoLimpa is { Length: > 200 })
            throw new DomainException("A descrição não pode passar de 200 caracteres.");

        return new Lancamento(Guid.NewGuid(), tipo, valor, data, descricaoLimpa, DateTime.UtcNow);
    }

    /// <summary>
    /// Impacto do lançamento no saldo: crédito soma, débito subtrai. Usado tanto
    /// no write side quanto na projeção do read side, mantendo a regra num só lugar.
    /// </summary>
    public decimal ImpactoNoSaldo() => Tipo == TipoLancamento.Credito ? Valor : -Valor;
}
