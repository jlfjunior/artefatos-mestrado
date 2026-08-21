using Entries.Domain.Entities;
using Entries.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entries.Infrastructure.Persistence.Configurations
{
    public sealed class EntryConfiguration : IEntityTypeConfiguration<Entry>
    {
        public void Configure(EntityTypeBuilder<Entry> builder)
        {
            builder.ToTable("entries");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id");

            builder.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion(
                    v => v.ToString(),
                    v => (EntryType)Enum.Parse(typeof(EntryType), v))
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.OwnsOne(e => e.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Ignore(e => e.DomainEvents);
        }
    }
}
