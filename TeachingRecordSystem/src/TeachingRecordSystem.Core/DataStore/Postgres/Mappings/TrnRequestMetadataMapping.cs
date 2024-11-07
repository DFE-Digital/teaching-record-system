using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrnRequestMetadataMapping : IEntityTypeConfiguration<TrnRequestMetadata>
{
    public void Configure(EntityTypeBuilder<TrnRequestMetadata> builder)
    {
        builder.HasKey(r => new { r.ApplicationUserId, r.RequestId });
        builder.Property(r => r.RequestId).IsRequired().HasMaxLength(TrnRequest.RequestIdMaxLength);
        builder.Property(o => o.VerifiedOneLoginUserSubject).HasMaxLength(255);
    }
}
