using FluxoDeCaixa.Caixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoDeCaixa.Caixa.Infra.Data.Mappings
{
    internal class EntryTypeMapping : IEntityTypeConfiguration<Entry>
    {
        public void Configure(EntityTypeBuilder<Entry> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("varchar(150)");

            builder.HasOne(e => e.User)
                .WithMany(u => u.Entries)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e => e.Account)
                .WithMany(a => a.Entries)
                .HasForeignKey(e => e.AccountId);
        }
    }
}
