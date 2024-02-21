using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Infrastructure.EntityFramework;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class InductionCompletedEmailsJobItemMapping : IEntityTypeConfiguration<InductionCompletedEmailsJobItem>
{
    public void Configure(EntityTypeBuilder<InductionCompletedEmailsJobItem> builder)
    {
        builder.ToTable("induction_completed_emails_job_items");
        builder.Property(i => i.InductionCompletedEmailsJobId).IsRequired();
        builder.Property(i => i.PersonId).IsRequired();
        builder.HasKey(i => new { i.InductionCompletedEmailsJobId, i.PersonId });
        builder.Property(i => i.Trn).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(i => i.EmailAddress).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Personalization).HasJsonConversion().IsRequired().HasColumnType("jsonb");
        builder.HasIndex(i => i.Personalization).HasMethod("gin");
        builder.HasOne(i => i.InductionCompletedEmailsJob).WithMany(j => j.JobItems).HasForeignKey(i => i.InductionCompletedEmailsJobId);
    }
}
