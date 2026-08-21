using ControleLancamentos.Domain.Lancamentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleLancamentos.Infrastructure.Persistencia.Configuracoes;

public class ConsolidadoDiarioConfiguracao : IEntityTypeConfiguration<ConsolidadoDiario>
{
    public void Configure(EntityTypeBuilder<ConsolidadoDiario> builder)
    {
        builder.ToTable("consolidados_diarios");

        builder.HasKey(x => x.DataReferencia);

        builder.Property(x => x.DataReferencia)
            .HasColumnName("data_referencia")
            .HasColumnType("date")
            .ValueGeneratedNever();

        builder.Property(x => x.TotalCreditos)
            .HasColumnName("total_creditos")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalDebitos)
            .HasColumnName("total_debitos")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Saldo)
            .HasColumnName("saldo")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.QuantidadeLancamentos)
            .HasColumnName("quantidade_lancamentos")
            .IsRequired();
    }
}
