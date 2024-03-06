using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class MandatoryQualificationMapping : IEntityTypeConfiguration<MandatoryQualification>
{
    public void Configure(EntityTypeBuilder<MandatoryQualification> builder)
    {
        builder.HasOne<MandatoryQualificationProvider>(q => q.Provider).WithMany().HasForeignKey(p => p.ProviderId).HasConstraintName("fk_qualifications_mandatory_qualification_provider");
        builder.Property(q => q.ProviderId).HasColumnName("mq_provider_id");
        builder.Property(q => q.Specialism).HasColumnName("mq_specialism");
        builder.Property(q => q.Status).HasColumnName("mq_status");
        builder.Property(q => q.StartDate).HasColumnName("start_date");
        builder.Property(q => q.EndDate).HasColumnName("end_date");
    }
}
