using ControleLancamentos.Application.Lancamentos.Eventos;
using ControleLancamentos.Application.Lancamentos.Repositorios;
using ControleLancamentos.Domain.Lancamentos;
using ControleLancamentos.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ControleLancamentos.Infrastructure.Repositorios;

public class ConsolidadoDiarioRepositorio(ControleLancamentosDbContext contexto) : IConsolidadoDiarioRepositorio
{
    public Task<ConsolidadoDiario?> ObterPorDataAsync(
        DateOnly dataReferencia,
        CancellationToken cancellationToken = default)
    {
        return contexto.ConsolidadosDiarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataReferencia == dataReferencia, cancellationToken);
    }

    public async Task AcumularLancamentoAsync(
        LancamentoCriadoEvent evento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evento);

        var totalCreditos = evento.Tipo == TipoLancamento.Credito ? evento.Valor : 0m;
        var totalDebitos = evento.Tipo == TipoLancamento.Debito ? evento.Valor : 0m;
        var saldo = evento.Tipo == TipoLancamento.Credito ? evento.Valor : -evento.Valor;

        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO consolidados_diarios (
                 data_referencia,
                 total_creditos,
                 total_debitos,
                 saldo,
                 quantidade_lancamentos
             ) VALUES (
                 {evento.DataReferencia},
                 {totalCreditos},
                 {totalDebitos},
                 {saldo},
                 1
             )
             ON CONFLICT (data_referencia) DO UPDATE
             SET total_creditos = consolidados_diarios.total_creditos + EXCLUDED.total_creditos,
                 total_debitos = consolidados_diarios.total_debitos + EXCLUDED.total_debitos,
                 saldo = consolidados_diarios.saldo + EXCLUDED.saldo,
                 quantidade_lancamentos = consolidados_diarios.quantidade_lancamentos + EXCLUDED.quantidade_lancamentos;
             """,
            cancellationToken);
    }
}
