using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class IntegrationTransactionMapping : IEntityTypeConfiguration<IntegrationTransaction>
{
    public void Configure(EntityTypeBuilder<IntegrationTransaction> builder)
    {
        builder.ToTable("integration_transactions");
        builder.HasKey(p => p.IntegrationTransactionId);
        builder.Property(p => p.InterfaceType).IsRequired();
        builder.Property(p => p.FileName).IsRequired();
        builder.Property(p => p.ImportStatus).IsRequired();
        builder.Property(p => p.SuccessCount).IsRequired();
        builder.Property(p => p.TotalCount).IsRequired();
        builder.Property(p => p.DuplicateCount).IsRequired();
        builder.Property(p => p.FailureCount).IsRequired();
        builder.Property(p => p.CreatedDate).IsRequired();
    }
}
