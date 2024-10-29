using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class QualifiedTeacherStatusQualificationMapping : IEntityTypeConfiguration<QualifiedTeacherStatusQualification>
{
    public void Configure(EntityTypeBuilder<QualifiedTeacherStatusQualification> builder)
    {
        builder.Property(q => q.AwardedDate).HasColumnName("awarded_date");
    }
}
