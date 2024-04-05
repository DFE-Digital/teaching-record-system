using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EstablishmentSourceMapping : IEntityTypeConfiguration<EstablishmentSource>
{
    public void Configure(EntityTypeBuilder<EstablishmentSource> builder)
    {
        builder.ToTable("establishment_sources");
        builder.HasKey(e => e.EstablishmentSourceId);
        builder.Property(e => e.Name).HasMaxLength(50).UseCollation("case_insensitive").IsRequired();
    }
}
