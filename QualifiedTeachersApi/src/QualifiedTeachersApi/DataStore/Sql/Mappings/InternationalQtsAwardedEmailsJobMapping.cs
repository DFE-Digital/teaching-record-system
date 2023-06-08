﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QualifiedTeachersApi.DataStore.Sql.Models;

namespace QualifiedTeachersApi.DataStore.Sql.Mappings;

public class InternationalQtsAwardedEmailsJobMapping : IEntityTypeConfiguration<InternationalQtsAwardedEmailsJob>
{
    public void Configure(EntityTypeBuilder<InternationalQtsAwardedEmailsJob> builder)
    {
        builder.ToTable("international_qts_awarded_emails_jobs");
        builder.Property(j => j.InternationalQtsAwardedEmailsJobId).IsRequired();
        builder.HasKey(j => j.InternationalQtsAwardedEmailsJobId);
        builder.Property(j => j.ExecutedUtc).IsRequired();
        builder.HasIndex(j => j.ExecutedUtc).HasDatabaseName("ix_international_qts_awarded_emails_jobs_executed_utc");
        builder.Property(j => j.AwardedToUtc).IsRequired();
    }
}
