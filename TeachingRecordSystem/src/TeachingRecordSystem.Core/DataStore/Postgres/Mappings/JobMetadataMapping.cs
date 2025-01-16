using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Infrastructure.EntityFramework;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class JobMetadataMapping : IEntityTypeConfiguration<JobMetadata>
{
    public void Configure(EntityTypeBuilder<JobMetadata> builder)
    {
        builder.ToTable("job_metadata");
        builder.HasKey(j => j.JobName);
        builder.Property(j => j.JobName).HasColumnName("job_name").HasMaxLength(200);
        builder.Property(j => j.Metadata).HasJsonConversion().IsRequired().HasColumnType("jsonb"); ;
    }
}
