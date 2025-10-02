using FluxoCaixa.Domain.Base;
using FluxoCaixa.Domain.Enums;

namespace FluxoCaixa.Domain.Entities;

public class Lancamento : Entity
{
    public DateTime Data { get; private set; }
    public decimal Valor { get; private set; }
    public string Tipo { get; private set; }
    public string? Descricao { get; private set; }

    private Lancamento() { }
    
    public Lancamento(DateTime data, decimal valor, TipoLancamento tipo)
    {
        Data = data;
        Valor = valor;
        Tipo = tipo.ToString();
    }
    public void Atualizar(string descricao, decimal valor, DateTime data)
    {
        Descricao = descricao;
        Valor = valor;
        Data = data;
    }

}
