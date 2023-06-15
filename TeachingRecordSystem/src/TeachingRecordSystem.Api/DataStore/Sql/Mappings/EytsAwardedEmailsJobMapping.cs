using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Api.DataStore.Sql.Models;

namespace TeachingRecordSystem.Api.DataStore.Sql.Mappings;

public class EytsAwardedEmailsJobMapping : IEntityTypeConfiguration<EytsAwardedEmailsJob>
{
    public void Configure(EntityTypeBuilder<EytsAwardedEmailsJob> builder)
    {
        builder.ToTable("eyts_awarded_emails_jobs");
        builder.Property(j => j.EytsAwardedEmailsJobId).IsRequired();
        builder.HasKey(j => j.EytsAwardedEmailsJobId);
        builder.Property(j => j.ExecutedUtc).IsRequired();
        builder.HasIndex(j => j.ExecutedUtc).HasDatabaseName("ix_eyts_awarded_emails_jobs_executed_utc");
        builder.Property(j => j.AwardedToUtc).IsRequired();
    }
}
