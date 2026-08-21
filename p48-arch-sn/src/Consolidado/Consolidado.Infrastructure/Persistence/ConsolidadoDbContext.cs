using Consolidado.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Consolidado.Infrastructure.Persistence;

public class ConsolidadoDbContext : DbContext
{
    public ConsolidadoDbContext(DbContextOptions<ConsolidadoDbContext> options) : base(options) { }

    public DbSet<SaldoDiario> SaldosDiarios => Set<SaldoDiario>();
    public DbSet<LancamentoProcessado> LancamentosProcessados => Set<LancamentoProcessado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SaldoDiario>(b =>
        {
            b.ToTable("saldos_diarios");
            b.HasKey(s => s.Data);
            b.Property(s => s.Data).HasColumnType("date");
            b.Property(s => s.TotalCreditos).HasColumnType("numeric(18,2)");
            b.Property(s => s.TotalDebitos).HasColumnType("numeric(18,2)");
            b.Property(s => s.Saldo).HasColumnType("numeric(18,2)");
            b.Property(s => s.AtualizadoEmUtc);
        });

        modelBuilder.Entity<LancamentoProcessado>(b =>
        {
            b.ToTable("lancamentos_processados");
            b.HasKey(l => l.LancamentoId);
            b.Property(l => l.ProcessadoEmUtc);
        });

        // Inbox do MassTransit (idempotência também na camada de mensageria).
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        base.OnModelCreating(modelBuilder);
    }
}
