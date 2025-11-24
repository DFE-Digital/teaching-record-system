using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ProcessEventMapping : IEntityTypeConfiguration<ProcessEvent>
{
    public void Configure(EntityTypeBuilder<ProcessEvent> builder)
    {
        builder.ToTable("process_events");
        builder.Property(e => e.EventName).HasMaxLength(200);
        builder.Property(e => e.Payload)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, IEvent.SerializerOptions),
                v => JsonSerializer.Deserialize<IEvent>(v, IEvent.SerializerOptions)!);
        builder.HasOne<Process>().WithMany(a => a.Events).HasForeignKey(ae => ae.ProcessId).HasConstraintName("fk_process_events_process_process_id");
        builder.HasIndex(e => new { e.PersonIds, e.EventName }).HasMethod("GIN").IsCreatedConcurrently();
        builder.HasIndex(e => e.ProcessId).IsCreatedConcurrently();
    }
}
