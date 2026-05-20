using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class OutboxMessageProcessorMetadataMapping : IEntityTypeConfiguration<OutboxMessageProcessorMetadata>
{
    public void Configure(EntityTypeBuilder<OutboxMessageProcessorMetadata> builder)
    {
        builder.ToTable("outbox_message_processor_metadata");
        builder.HasKey(o => o.Key);
        builder.Property(o => o.Key).HasMaxLength(200);
    }
}
