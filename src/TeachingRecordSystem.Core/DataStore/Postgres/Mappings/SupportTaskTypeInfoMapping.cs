using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class SupportTaskTypeInfoMapping : IEntityTypeConfiguration<SupportTaskTypeInfo>
{
    public void Configure(EntityTypeBuilder<SupportTaskTypeInfo> builder)
    {
        builder.IncludeInAnalyticsSync(hidden: false);
        builder.ToTable("support_task_types");
        builder.HasKey(t => t.SupportTaskType);
        builder.Property(t => t.Name).HasMaxLength(200);
    }
}
