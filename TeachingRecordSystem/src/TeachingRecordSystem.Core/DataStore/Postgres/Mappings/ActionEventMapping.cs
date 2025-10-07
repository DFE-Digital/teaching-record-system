using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Action = TeachingRecordSystem.Core.DataStore.Postgres.Models.Action;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ActionEventMapping : IEntityTypeConfiguration<ActionEvent>
{
    public void Configure(EntityTypeBuilder<ActionEvent> builder)
    {
        builder.ToTable("action_events");
        builder.Property(e => e.Payload)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, typeof(IEvent), IEvent.SerializerOptions),
                v => JsonSerializer.Deserialize<IEvent>(v, IEvent.SerializerOptions)!);
        builder.HasOne<Action>().WithMany(a => a.Events).HasForeignKey(ae => ae.ActionId);
        builder.HasIndex(e => e.PersonIds).HasMethod("GIN");
    }
}
