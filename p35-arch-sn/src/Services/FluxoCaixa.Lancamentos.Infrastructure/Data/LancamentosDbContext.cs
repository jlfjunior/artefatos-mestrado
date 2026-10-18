using FluxoCaixa.Lancamentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoCaixa.Lancamentos.Infrastructure.Data;

public class LancamentosDbContext : DbContext
{
    public LancamentosDbContext(DbContextOptions<LancamentosDbContext> options) : base(options) { }

    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new LancamentoConfiguration());
    }
}

public class LancamentoConfiguration : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Valor).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<string>().IsRequired();
        builder.Property(x => x.DataHora).IsRequired();
        builder.Property(x => x.CriadoEm).IsRequired();

        // Indíce pra ajudar na consulta por data
        builder.HasIndex(x => x.DataHora);
    }
}
