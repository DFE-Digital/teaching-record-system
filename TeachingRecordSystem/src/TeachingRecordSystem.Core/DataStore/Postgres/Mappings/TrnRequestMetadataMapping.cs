using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrnRequestMetadataMapping : IEntityTypeConfiguration<TrnRequestMetadata>
{
    public void Configure(EntityTypeBuilder<TrnRequestMetadata> builder)
    {
        builder.HasKey(r => new { r.ApplicationUserId, r.RequestId });
        builder.Property(r => r.RequestId).IsRequired().HasMaxLength(TrnRequest.RequestIdMaxLength);
        builder.Property(r => r.OneLoginUserSubject).HasMaxLength(255);
        builder.HasIndex(r => r.OneLoginUserSubject);
        builder.HasIndex(r => r.EmailAddress);
        builder.HasOne(r => r.ApplicationUser).WithMany().HasForeignKey(r => r.ApplicationUserId);
        builder.OwnsOne(r => r.Matches, m => m.ToJson().OwnsMany(m => m.MatchedPersons));
    }
}
