using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class QualificationMapping : IEntityTypeConfiguration<Qualification>
{
    public void Configure(EntityTypeBuilder<Qualification> builder)
    {
        builder.ToTable("qualifications");
        builder.HasKey(q => q.QualificationId);
        builder.HasQueryFilter(q => EF.Property<DateTime?>(q, nameof(Qualification.DeletedOn)) != null);
        builder.HasDiscriminator(q => q.QualificationType)
            .HasValue<MandatoryQualification>(QualificationType.MandatoryQualification);
        builder.HasOne<Person>().WithMany().HasForeignKey(q => q.PersonId).HasConstraintName("fk_qualifications_person");
        builder.HasOne<User>().WithMany().HasForeignKey(q => q.CreatedByUserId).HasConstraintName("fk_qualifications_created_by");
        builder.HasOne<User>().WithMany().HasForeignKey(q => q.UpdatedByUserId).HasConstraintName("fk_qualifications_updated_by");
        builder.HasOne<User>().WithMany().HasForeignKey(q => q.DeletedByUserId).HasConstraintName("fk_qualifications_deleted_by");
        builder.HasIndex(q => q.PersonId);
        builder.HasIndex(q => q.DqtQualificationId).HasFilter("dqt_qualification_id is not null").IsUnique();
    }
}
