using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class WebhookEndpointMapping : IEntityTypeConfiguration<WebhookEndpoint>
{
    public void Configure(EntityTypeBuilder<WebhookEndpoint> builder)
    {
        builder.HasQueryFilter(e => EF.Property<DateTime?>(e, nameof(WebhookEndpoint.DeletedOn)) == null);
        builder.Property(e => e.Address).HasMaxLength(200);
        builder.Property(e => e.ApiVersion).HasMaxLength(50);
        builder.HasOne(e => e.ApplicationUser).WithMany().HasForeignKey(e => e.ApplicationUserId);
    }
}
