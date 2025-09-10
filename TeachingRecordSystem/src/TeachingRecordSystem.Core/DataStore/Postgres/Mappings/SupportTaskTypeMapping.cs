using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class SupportTaskTypeMapping : IEntityTypeConfiguration<SupportTaskType>
{
    public void Configure(EntityTypeBuilder<SupportTaskType> builder)
    {
        builder.ToTable("support_task_types");
        builder.HasKey(t => t.SupportTaskTypeId);
        builder.Property(t => t.Name).HasMaxLength(200);
        builder.Property(t => t.Category);
        builder.Ignore(t => t.Title);
        builder.HasData(SupportTaskType.All);
    }
}
