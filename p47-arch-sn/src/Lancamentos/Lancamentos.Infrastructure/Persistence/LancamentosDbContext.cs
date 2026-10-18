using Lancamentos.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.Infrastructure.Persistence;

public class LancamentosDbContext : DbContext
{
    public LancamentosDbContext(DbContextOptions<LancamentosDbContext> options) : base(options) { }

    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lancamento>(b =>
        {
            b.ToTable("lancamentos");
            b.HasKey(l => l.Id);
            b.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(10).IsRequired();
            b.Property(l => l.Valor).HasColumnType("numeric(18,2)").IsRequired();
            b.Property(l => l.Data).IsRequired();
            b.Property(l => l.Descricao).HasMaxLength(200);
            b.Property(l => l.CriadoEmUtc).IsRequired();
        });

        // Tabelas do outbox do MassTransit (InboxState, OutboxState, OutboxMessage).
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        base.OnModelCreating(modelBuilder);
    }
}
