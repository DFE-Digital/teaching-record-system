using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ProcessMapping : IEntityTypeConfiguration<Process>
{
    public void Configure(EntityTypeBuilder<Process> builder)
    {
        builder.ToTable("processes");
        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
        builder.HasIndex(p => p.ProcessType);
        builder.HasIndex(p => p.PersonIds).HasMethod("GIN");
        builder.Property(p => p.ChangeReason)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, IChangeReasonInfo.SerializerOptions),
                v => JsonSerializer.Deserialize<IChangeReasonInfo>(v, IChangeReasonInfo.SerializerOptions)!);
    }
}
