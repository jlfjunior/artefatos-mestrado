using ControleLancamentos.Domain.Lancamentos;
using Microsoft.EntityFrameworkCore;

namespace ControleLancamentos.Infrastructure.Persistencia;

public class ControleLancamentosDbContext(DbContextOptions<ControleLancamentosDbContext> options) : DbContext(options)
{
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<ConsolidadoDiario> ConsolidadosDiarios => Set<ConsolidadoDiario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configuracoes.LancamentoConfiguracao());
        modelBuilder.ApplyConfiguration(new Configuracoes.ConsolidadoDiarioConfiguracao());

        base.OnModelCreating(modelBuilder);
    }
}
