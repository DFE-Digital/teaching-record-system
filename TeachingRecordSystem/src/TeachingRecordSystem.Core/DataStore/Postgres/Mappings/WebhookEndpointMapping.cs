using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class WebhookEndpointMapping : IEntityTypeConfiguration<WebhookEndpoint>
{
    public void Configure(EntityTypeBuilder<WebhookEndpoint> builder)
    {
        builder.Property(e => e.Address).HasMaxLength(200);
        builder.Property(e => e.ApiVersion).HasMaxLength(50);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(e => e.ApplicationUserId);
    }
}
