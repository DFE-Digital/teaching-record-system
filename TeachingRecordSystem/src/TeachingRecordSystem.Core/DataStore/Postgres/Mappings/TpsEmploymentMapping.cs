using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TpsEmploymentMapping : IEntityTypeConfiguration<TpsEmployment>
{
    public void Configure(EntityTypeBuilder<TpsEmployment> builder)
    {
        builder.ToTable("tps_employments");
        builder.HasKey(e => e.TpsEmploymentId);
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.LastKnownTpsEmployedDate).IsRequired();
        builder.Property(e => e.EmploymentType).IsRequired();
        builder.Property(e => e.WithdrawalConfirmed).IsRequired();
        builder.Property(e => e.CreatedOn).IsRequired();
        builder.Property(e => e.UpdatedOn).IsRequired();
        builder.Property(e => e.Key).HasMaxLength(50).IsRequired();
        builder.Property(e => e.NationalInsuranceNumber).HasMaxLength(9).IsFixedLength();
        builder.Property(e => e.PersonPostcode).HasMaxLength(10);
        builder.Property(e => e.PersonEmailAddress).HasMaxLength(100);
        builder.Property(e => e.EmployerPostcode).HasMaxLength(10);
        builder.Property(e => e.EmployerEmailAddress).HasMaxLength(100);
        builder.HasIndex(e => e.Key).HasDatabaseName(TpsEmployment.KeyIndexName);
        builder.HasIndex(e => e.PersonId).HasDatabaseName(TpsEmployment.PersonIdIndexName);
        builder.HasIndex(e => e.EstablishmentId).HasDatabaseName(TpsEmployment.EstablishmentIdIndexName);
        builder.HasOne<Person>().WithMany().HasForeignKey(e => e.PersonId).HasConstraintName("fk_tps_employments_person_id");
        builder.HasOne<Establishment>().WithMany().HasForeignKey(e => e.EstablishmentId).HasConstraintName("fk_tps_employments_establishment_id");
    }
}
