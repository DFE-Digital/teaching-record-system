using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EventMapping : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");
        builder.Property(e => e.EventName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Created).IsRequired();
        builder.Property(e => e.Inserted).IsRequired();
        builder.Property(e => e.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.Published);
        builder.HasKey(e => e.EventId);
        builder.Property(e => e.PersonIds);
        builder.HasIndex(e => new { e.PersonId, e.EventName }).HasFilter("person_id is not null");
        builder.HasIndex(e => new { e.PersonIds, e.EventName }).HasDatabaseName("ix_events_person_ids").HasMethod("gin").IsCreatedConcurrently();
        builder.HasIndex(e => new { e.EventName, e.Created }).IsCreatedConcurrently();
    }
}
