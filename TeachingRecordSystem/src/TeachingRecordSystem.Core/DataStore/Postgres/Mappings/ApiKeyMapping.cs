using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ApiKeyMapping : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(k => k.ApiKeyId);
        builder.Property(k => k.Key).HasMaxLength(ApiKey.KeyMaxLength);
        builder.HasOne(k => k.ApplicationUser).WithMany(u => u.ApiKeys).HasForeignKey(k => k.ApplicationUserId).HasConstraintName("fk_api_key_application_user");
        builder.HasIndex(k => k.Key).IsUnique().HasDatabaseName(ApiKey.KeyUniqueIndexName);
        builder.HasIndex(k => k.ApplicationUserId);
    }
}
