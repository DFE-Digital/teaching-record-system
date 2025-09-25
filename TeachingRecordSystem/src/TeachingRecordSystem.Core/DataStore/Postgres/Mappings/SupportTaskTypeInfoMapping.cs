using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class SupportTaskTypeInfoMapping : IEntityTypeConfiguration<SupportTaskTypeInfo>
{
    public void Configure(EntityTypeBuilder<SupportTaskTypeInfo> builder)
    {
        builder.ToTable("support_task_types");
        builder.HasKey(t => t.SupportTaskType);
        builder.Property(t => t.Name).HasMaxLength(200);

        builder.HasData(SupportTaskTypeRegistry.All.Select(i => new SupportTaskTypeInfo { SupportTaskType = i.SupportTaskType, Name = i.Name }));
    }
}
