using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TpsEstablishmentMapping : IEntityTypeConfiguration<TpsEstablishment>
{
    public void Configure(EntityTypeBuilder<TpsEstablishment> builder)
    {
        builder.ToTable("tps_establishments");
        builder.HasKey(e => e.TpsEstablishmentId);
        builder.Property(e => e.LaCode).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(e => e.EstablishmentCode).HasMaxLength(4).IsFixedLength().IsRequired();
        builder.Property(e => e.EmployersName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SchoolGiasName).HasMaxLength(200);
        builder.HasIndex(e => new { e.LaCode, e.EstablishmentCode }).HasDatabaseName(TpsEstablishment.LaCodeEstablishmentCodeIndexName);
    }
}
