using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class MandatoryQualificationMapping : IEntityTypeConfiguration<MandatoryQualification>
{
    public void Configure(EntityTypeBuilder<MandatoryQualification> builder)
    {
        builder.HasOne<MandatoryQualificationProvider>(q => q.Provider).WithMany().HasForeignKey(p => p.ProviderId).HasConstraintName("fk_qualifications_mandatory_qualification_provider");
        builder.Navigation(q => q.Provider).AutoInclude();
        builder.Property(q => q.ProviderId).HasColumnName("mq_provider_id");
        builder.Property(q => q.Specialism).HasColumnName("mq_specialism");
        builder.Property(q => q.Status).HasColumnName("mq_status");
        builder.Property(q => q.StartDate).HasColumnName("start_date");
        builder.Property(q => q.EndDate).HasColumnName("end_date");
        builder.HasIndex(q => q.DqtQualificationId).HasFilter("dqt_qualification_id is not null").IsUnique();
        builder.Property(q => q.DqtMqEstablishmentValue).HasMaxLength(3);
        builder.Property(q => q.DqtSpecialismValue).HasMaxLength(3);
    }
}
