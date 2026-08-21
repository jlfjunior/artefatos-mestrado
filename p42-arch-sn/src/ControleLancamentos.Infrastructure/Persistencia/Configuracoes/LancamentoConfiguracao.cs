using ControleLancamentos.Domain.Lancamentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleLancamentos.Infrastructure.Persistencia.Configuracoes;

public class LancamentoConfiguracao : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("lancamentos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Valor)
            .HasColumnName("valor")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DataLancamento)
            .HasColumnName("data_lancamento")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200);
    }
}
