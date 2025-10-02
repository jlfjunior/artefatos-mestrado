using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Enums;

namespace FluxoCaixa.Tests.Integration;

public static class LancamentoMock
{
    public static Lancamento CriarReceitaCredito()
    {
        return new Lancamento(DateTime.Now, 100, TipoLancamento.Credito);
    }

    public static Lancamento CriarDespesaDebito()
    {
        return new Lancamento(DateTime.Now, 50, TipoLancamento.Debito);
    }
}
