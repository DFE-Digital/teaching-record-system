using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrnRequestMetadataMapping : IEntityTypeConfiguration<TrnRequestMetadata>
{
    public void Configure(EntityTypeBuilder<TrnRequestMetadata> builder)
    {
        builder.IncludeInAnalyticsSync(includeAllColumns: false);
        builder.HasKey(r => new { r.ApplicationUserId, r.RequestId });
        builder.Property(r => r.ResolvedPersonId).ConfigureAnalyticsSync(hidden: true);
        builder.Property(r => r.ApplicationUserId).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.RequestId).IsRequired().HasMaxLength(TrnRequest.RequestIdMaxLength).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.CreatedOn).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.IdentityVerified).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.OneLoginUserSubject).HasMaxLength(255).ConfigureAnalyticsSync(hidden: true);
        builder.Property(r => r.ResolvedPersonId).ConfigureAnalyticsSync(hidden: true);
        builder.Property(r => r.Status).ConfigureAnalyticsSync(hidden: false);
        builder.Property(r => r.AddressLine1).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.AddressLine2).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.AddressLine3).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.City).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.Country).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.Postcode).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.FirstName).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.MiddleName).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.LastName).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.PreviousFirstName).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.PreviousMiddleName).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.PreviousLastName).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.DateOfBirth).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(r => r.EmailAddress).ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.HasIndex(r => r.OneLoginUserSubject);
        builder.HasIndex(r => r.EmailAddress);
        builder.HasOne(r => r.ApplicationUser).WithMany().HasForeignKey(r => r.ApplicationUserId);
        builder.HasIndex(r => r.ApplicationUserId).IsCreatedConcurrently();
    }
}
