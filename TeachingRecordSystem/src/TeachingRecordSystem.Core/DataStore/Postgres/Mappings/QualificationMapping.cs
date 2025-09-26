using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class QualificationMapping : IEntityTypeConfiguration<Qualification>
{
    public void Configure(EntityTypeBuilder<Qualification> builder)
    {
        builder.ToTable("qualifications");
        builder.HasKey(q => q.QualificationId);
        builder.HasQueryFilter(q => EF.Property<DateTime?>(q, nameof(Qualification.DeletedOn)) == null);
        builder.HasDiscriminator(q => q.QualificationType)
            .HasValue<MandatoryQualification>(QualificationType.MandatoryQualification)
            .HasValue<RouteToProfessionalStatus>(QualificationType.RouteToProfessionalStatus);
        builder.HasOne(q => q.Person).WithMany(p => p.Qualifications).HasForeignKey(q => q.PersonId).HasConstraintName(Qualification.PersonForeignKeyName);
        builder.HasIndex(q => q.PersonId);
    }
}
