using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EstablishmentSourceMapping : IEntityTypeConfiguration<EstablishmentSource>
{
    public void Configure(EntityTypeBuilder<EstablishmentSource> builder)
    {
        builder.IncludeInAnalyticsSync(hidden: false);
        builder.ToTable("establishment_sources");
        builder.HasKey(e => e.EstablishmentSourceId);
        builder.Property(e => e.Name).HasMaxLength(50).UseCollation(Collations.CaseInsensitive).IsRequired();
    }
}
