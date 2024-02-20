using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrnRequestMapping : IEntityTypeConfiguration<TrnRequest>
{
    public void Configure(EntityTypeBuilder<TrnRequest> builder)
    {
        builder.Property(r => r.ClientId).IsRequired().HasMaxLength(50);
        builder.Property(r => r.RequestId).IsRequired().HasMaxLength(TrnRequest.RequestIdMaxLength);
        builder.Property(t => t.TrnToken).HasMaxLength(TrnRequest.TrnTokenMaxLength);
        builder.HasIndex(r => new { r.ClientId, r.RequestId }).IsUnique();
    }
}
