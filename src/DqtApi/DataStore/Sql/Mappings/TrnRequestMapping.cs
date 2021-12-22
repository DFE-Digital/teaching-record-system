using DqtApi.DataStore.Sql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DqtApi.DataStore.Sql.Mappings
{
    public class TrnRequestMapping : IEntityTypeConfiguration<TrnRequest>
    {
        public void Configure(EntityTypeBuilder<TrnRequest> builder)
        {
            builder.Property(r => r.ClientId).IsRequired().HasMaxLength(50);
            builder.Property(r => r.RequestId).IsRequired().HasMaxLength(TrnRequest.RequestIdMaxLength);
            builder.HasIndex(r => new { r.ClientId, r.RequestId }).IsUnique();
        }
    }
}
