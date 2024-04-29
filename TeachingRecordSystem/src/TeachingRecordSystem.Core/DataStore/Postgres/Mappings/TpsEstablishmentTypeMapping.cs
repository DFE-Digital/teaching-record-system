using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TpsEstablishmentTypeMapping : IEntityTypeConfiguration<TpsEstablishmentType>
{
    public void Configure(EntityTypeBuilder<TpsEstablishmentType> builder)
    {
        builder.ToTable("tps_establishment_types");
        builder.HasKey(e => e.TpsEstablishmentTypeId);
        builder.Property(e => e.EstablishmentRangeFrom).HasMaxLength(4).IsFixedLength().IsRequired();
        builder.Property(e => e.EstablishmentRangeTo).HasMaxLength(4).IsFixedLength().IsRequired();
        builder.Property(e => e.Description).HasMaxLength(300).IsRequired();
        builder.Property(e => e.ShortDescription).HasMaxLength(120).IsRequired();
    }
}
