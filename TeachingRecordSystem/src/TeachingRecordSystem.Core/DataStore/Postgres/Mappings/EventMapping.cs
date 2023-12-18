using Microsoft.EntityFrameworkCore;
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
        builder.Property(e => e.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.Published);
        builder.HasKey(e => e.EventId);
        builder.HasIndex(e => e.Payload).HasMethod("gin");
    }
}
