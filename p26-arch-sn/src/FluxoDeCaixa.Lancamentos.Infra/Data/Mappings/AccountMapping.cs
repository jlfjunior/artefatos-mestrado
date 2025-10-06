using FluxoDeCaixa.Caixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoDeCaixa.Caixa.Infra.Data.Mappings
{
    public class AccountMapping : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);

            builder.ToTable("Account");

            builder.HasMany(a => a.Entries)
                .WithOne(e => e.Account)
                .HasForeignKey(e => e.AccountId);
        }
    }
}
