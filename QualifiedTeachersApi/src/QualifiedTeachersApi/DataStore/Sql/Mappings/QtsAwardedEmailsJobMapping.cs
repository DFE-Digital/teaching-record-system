﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QualifiedTeachersApi.DataStore.Sql.Models;

namespace QualifiedTeachersApi.DataStore.Sql.Mappings;

public class QtsAwardedEmailsJobMapping : IEntityTypeConfiguration<QtsAwardedEmailsJob>
{
    public void Configure(EntityTypeBuilder<QtsAwardedEmailsJob> builder)
    {
        builder.ToTable("qts_awarded_emails_jobs");
        builder.Property(j => j.QtsAwardedEmailsJobId).IsRequired();
        builder.HasKey(j => j.QtsAwardedEmailsJobId);
        builder.Property(j => j.ExecutedUtc).IsRequired();
        builder.HasIndex(j => j.ExecutedUtc).HasDatabaseName("ix_qts_awarded_emails_jobs_executed_utc");
        builder.Property(j => j.AwardedToUtc).IsRequired();
    }
}
