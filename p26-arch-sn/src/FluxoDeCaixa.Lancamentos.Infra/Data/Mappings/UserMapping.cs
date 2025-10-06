using FluxoDeCaixa.Caixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoDeCaixa.Caixa.Infra.Data.Mappings
{
    internal class UserMapping : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(u => u.Cpf)
                .IsRequired()
                .HasColumnType("varchar(11)");

            builder.Property(u => u.Email)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.HasMany(u => u.Entries)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.User);
        }
    }
}
