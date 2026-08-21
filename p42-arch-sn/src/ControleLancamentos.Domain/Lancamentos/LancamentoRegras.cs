namespace ControleLancamentos.Domain.Lancamentos;

public static class LancamentoRegras
{
    private const int TamanhoMaximoDescricao = 200;

    public static Lancamento Criar(Guid id, decimal valor, TipoLancamento tipo, DateTime dataLancamento, string? descricao = null)
    {
        ValidarId(id);
        ValidarValor(valor);
        ValidarTipo(tipo);
        ValidarData(dataLancamento);

        return new Lancamento
        {
            Id = id,
            Valor = valor,
            Tipo = tipo,
            DataLancamento = dataLancamento,
            Descricao = NormalizarDescricao(descricao)
        };
    }

    public static void ValidarDados(decimal valor, TipoLancamento tipo, DateTime dataLancamento, string? descricao = null)
    {
        ValidarValor(valor);
        ValidarTipo(tipo);
        ValidarData(dataLancamento);
        NormalizarDescricao(descricao);
    }

    public static decimal ObterValorComSinal(decimal valor, TipoLancamento tipo)
    {
        ValidarValor(valor);
        ValidarTipo(tipo);

        return tipo == TipoLancamento.Credito ? valor : -valor;
    }

    public static void Atualizar(Lancamento lancamento, decimal valor, TipoLancamento tipo, DateTime dataLancamento, string? descricao = null)
    {
        ArgumentNullException.ThrowIfNull(lancamento);

        ValidarValor(valor);
        ValidarTipo(tipo);
        ValidarData(dataLancamento);

        lancamento.Valor = valor;
        lancamento.Tipo = tipo;
        lancamento.DataLancamento = dataLancamento;
        lancamento.Descricao = NormalizarDescricao(descricao);
    }

    private static void ValidarId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do lançamento é obrigatório.", nameof(id));
        }
    }

    private static void ValidarValor(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor do lançamento deve ser maior que zero.");
        }
    }

    private static void ValidarTipo(TipoLancamento tipo)
    {
        if (!Enum.IsDefined(typeof(TipoLancamento), tipo))
        {
            throw new ArgumentOutOfRangeException(nameof(tipo), "O tipo de lançamento informado é inválido.");
        }
    }

    private static void ValidarData(DateTime dataLancamento)
    {
        if (dataLancamento == default)
        {
            throw new ArgumentException("A data do lançamento é obrigatória.", nameof(dataLancamento));
        }
    }

    private static string? NormalizarDescricao(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            return null;
        }

        var descricaoNormalizada = descricao.Trim();

        if (descricaoNormalizada.Length > TamanhoMaximoDescricao)
        {
            throw new ArgumentException(
                $"A descrição do lançamento deve ter no máximo {TamanhoMaximoDescricao} caracteres.",
                nameof(descricao));
        }

        return descricaoNormalizada;
    }
}
