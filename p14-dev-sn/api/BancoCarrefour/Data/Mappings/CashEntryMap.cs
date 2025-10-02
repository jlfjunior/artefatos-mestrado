using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Mappings
{
    public class CashEntryMap : IEntityTypeConfiguration<CashEntry>
    {
        public void Configure(EntityTypeBuilder<CashEntry> builder)
        {
            builder.ToTable("CashEntries");

            builder.HasKey(ce => ce.Id);
            builder.Property(ce => ce.Id);

            builder.Property(ce => ce.DateCreated);
            builder.Property(ce => ce.EntryType);
            builder.Property(ce => ce.TransactionType);
            builder.Property(ce => ce.Value);

        }
    }
}
