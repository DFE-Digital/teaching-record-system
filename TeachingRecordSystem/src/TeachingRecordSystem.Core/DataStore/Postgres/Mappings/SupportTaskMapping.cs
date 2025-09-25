using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class SupportTaskMapping : IEntityTypeConfiguration<SupportTask>
{
    public void Configure(EntityTypeBuilder<SupportTask> builder)
    {
        builder.ToTable("support_tasks");
        builder.HasKey(p => p.SupportTaskReference);
        builder.Property(p => p.SupportTaskReference).HasMaxLength(16);
        builder.HasOne<Person>(t => t.Person).WithMany().HasForeignKey(p => p.PersonId).HasConstraintName("fk_support_tasks_person");
        builder.HasIndex(t => t.OneLoginUserSubject);
        builder.HasIndex(t => t.PersonId);
        builder.Property(t => t.Data)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                v => JsonSerializer.Serialize(v, typeof(ISupportTaskData), ISupportTaskData.SerializerOptions),
                v => JsonSerializer.Deserialize<ISupportTaskData>(v, ISupportTaskData.SerializerOptions)!);
        builder.HasOne(t => t.TrnRequestMetadata).WithMany().HasForeignKey(p => new { p.TrnRequestApplicationUserId, p.TrnRequestId });
        builder.HasOne<SupportTaskTypeInfo>().WithMany().HasForeignKey(t => t.SupportTaskType);
    }
}
