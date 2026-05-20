using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrnRequestMetadataMapping : IEntityTypeConfiguration<TrnRequestMetadata>
{
    public void Configure(EntityTypeBuilder<TrnRequestMetadata> builder)
    {
        builder.IncludeInAnalyticsSync(hidden: true);
        builder.HasKey(r => new { r.ApplicationUserId, r.RequestId });
        builder.Property(r => r.ApplicationUserId).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.RequestId).IsRequired().HasMaxLength(TrnRequest.RequestIdMaxLength).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.CreatedOn).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.IdentityVerified).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.OneLoginUserSubject).HasMaxLength(255);
        builder.Property(r => r.ResolvedPersonId).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.Status).ConfigureAnalyticsSync(hidden: false);
        builder.HasIndex(r => r.OneLoginUserSubject);
        builder.HasIndex(r => r.EmailAddress);
        builder.HasOne(r => r.ApplicationUser).WithMany().HasForeignKey(r => r.ApplicationUserId);
    }
}
