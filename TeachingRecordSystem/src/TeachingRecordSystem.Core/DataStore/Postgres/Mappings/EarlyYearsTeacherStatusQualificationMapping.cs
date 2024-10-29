using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EarlyYearsTeacherStatusQualificationMapping : IEntityTypeConfiguration<EarlyYearsTeacherStatusQualification>
{
    public void Configure(EntityTypeBuilder<EarlyYearsTeacherStatusQualification> builder)
    {
        builder.Property(q => q.AwardedDate).HasColumnName("awarded_date");
    }
}
