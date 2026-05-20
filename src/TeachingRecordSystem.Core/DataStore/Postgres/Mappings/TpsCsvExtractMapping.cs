using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TpsCsvExtractMapping : IEntityTypeConfiguration<TpsCsvExtract>
{
    public void Configure(EntityTypeBuilder<TpsCsvExtract> builder)
    {
        builder.ToTable("tps_csv_extracts");
        builder.HasKey(e => e.TpsCsvExtractId);
        builder.Property(e => e.Filename).IsRequired().HasMaxLength(TpsCsvExtract.FilenameMaxLength);
        builder.Property(e => e.CreatedOn).IsRequired();
    }
}
