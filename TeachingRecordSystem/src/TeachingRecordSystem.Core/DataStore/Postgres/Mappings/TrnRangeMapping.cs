using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrnRangeMapping : IEntityTypeConfiguration<TrnRange>
{
    public void Configure(EntityTypeBuilder<TrnRange> builder)
    {
        builder.ToTable("trn_ranges");
        builder.HasKey(r => r.FromTrn);
        builder.HasIndex(r => r.FromTrn).HasDatabaseName("ix_trn_ranges_unexhausted_trn_ranges").HasFilter("is_exhausted IS FALSE");
        builder.Property(r => r.FromTrn).IsRequired();
        builder.Property(r => r.ToTrn).IsRequired();
        builder.Property(r => r.NextTrn).IsRequired();
        builder.Property(r => r.IsExhausted).IsRequired();
    }
}
