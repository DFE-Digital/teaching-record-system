using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class InductionCompletedEmailsJobMapping : IEntityTypeConfiguration<InductionCompletedEmailsJob>
{
    public void Configure(EntityTypeBuilder<InductionCompletedEmailsJob> builder)
    {
        builder.ToTable("induction_completed_emails_jobs");
        builder.Property(j => j.InductionCompletedEmailsJobId).IsRequired();
        builder.HasKey(j => j.InductionCompletedEmailsJobId);
        builder.Property(j => j.ExecutedUtc).IsRequired();
        builder.HasIndex(j => j.ExecutedUtc).HasDatabaseName("ix_induction_completed_emails_jobs_executed_utc");
        builder.Property(j => j.AwardedToUtc).IsRequired();
    }
}
