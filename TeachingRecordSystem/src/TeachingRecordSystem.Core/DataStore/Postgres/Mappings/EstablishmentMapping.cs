using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EstablishmentMapping : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.ToTable("establishments");
        builder.HasKey(e => e.EstablishmentId);
        builder.Property(e => e.EstablishmentSourceId).IsRequired().HasDefaultValue(1);
        builder.HasIndex(e => e.Urn).HasDatabaseName(Establishment.UrnIndexName);
        builder.HasIndex(e => new { e.LaCode, e.EstablishmentNumber }).HasDatabaseName(Establishment.LaCodeEstablishmentNumberIndexName);
        builder.Property(e => e.Urn).HasMaxLength(6).IsFixedLength();
        builder.Property(e => e.LaCode).HasMaxLength(3).IsFixedLength();
        builder.Property(e => e.LaName).HasMaxLength(50).UseCollation("case_insensitive");
        builder.Property(e => e.EstablishmentNumber).HasMaxLength(4).IsFixedLength();
        builder.Property(e => e.EstablishmentName).HasMaxLength(120).UseCollation("case_insensitive");
        builder.Property(e => e.EstablishmentTypeCode).HasMaxLength(3);
        builder.Property(e => e.EstablishmentTypeName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(e => e.EstablishmentTypeGroupName).HasMaxLength(50);
        builder.Property(e => e.EstablishmentStatusName).HasMaxLength(50);
        builder.Property(e => e.Street).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(e => e.Locality).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(e => e.Address3).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(e => e.Town).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(e => e.County).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(e => e.Postcode).HasMaxLength(10).UseCollation("case_insensitive");
        builder.HasIndex(e => e.EstablishmentSourceId).HasDatabaseName(Establishment.EstablishmentSourceIdIndexName);
        builder.HasOne<EstablishmentSource>().WithMany().HasForeignKey(e => e.EstablishmentSourceId).HasConstraintName("fk_establishments_establishment_source_id");
    }
}
