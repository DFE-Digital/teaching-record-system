using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Infrastructure.EntityFramework;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EytsAwardedEmailsJobItemMapping : IEntityTypeConfiguration<EytsAwardedEmailsJobItem>
{
    public void Configure(EntityTypeBuilder<EytsAwardedEmailsJobItem> builder)
    {
        builder.ToTable("eyts_awarded_emails_job_items");
        builder.Property(i => i.EytsAwardedEmailsJobId).IsRequired();
        builder.Property(i => i.PersonId).IsRequired();
        builder.HasKey(i => new { i.EytsAwardedEmailsJobId, i.PersonId });
        builder.Property(i => i.Trn).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(i => i.EmailAddress).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Personalization).HasJsonConversion().IsRequired().HasColumnType("jsonb");
        builder.HasIndex(i => i.Personalization).HasMethod("gin");
        builder.HasOne(i => i.EytsAwardedEmailsJob).WithMany(j => j.JobItems).HasForeignKey(i => i.EytsAwardedEmailsJobId);
        builder.HasIndex(i => i.Trn);
    }
}
