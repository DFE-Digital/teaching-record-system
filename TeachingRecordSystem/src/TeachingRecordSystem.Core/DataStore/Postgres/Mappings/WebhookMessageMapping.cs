using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class WebhookMessageMapping : IEntityTypeConfiguration<WebhookMessage>
{
    public void Configure(EntityTypeBuilder<WebhookMessage> builder)
    {
        builder.HasOne(m => m.WebhookEndpoint).WithMany().HasForeignKey(m => m.WebhookEndpointId);
        builder.Property(m => m.CloudEventId).HasMaxLength(50);
        builder.Property(m => m.CloudEventType).HasMaxLength(100);
        builder.Property(m => m.ApiVersion).HasMaxLength(50);
    }
}
