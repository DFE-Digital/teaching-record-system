using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class IntegrationTransactionRecordMapping : IEntityTypeConfiguration<IntegrationTransactionRecord>
{
    public void Configure(EntityTypeBuilder<IntegrationTransactionRecord> builder)
    {
        builder.ToTable("integration_transaction_records");
        builder.HasKey(p => p.IntegrationTransactionRecordId);
        builder.Property(p => p.Duplicate);
        builder.Property(p => p.FailureMessage).HasMaxLength(3000);
        builder.Property(p => p.RowData).HasMaxLength(3000);
        builder.Property(p => p.CreatedDate).IsRequired();
        builder.Property(p => p.PersonId).IsRequired();
        builder.HasOne(p => p.Person)
            .WithMany()
            .HasForeignKey(p => p.PersonId);
        builder
            .HasOne(p => p.IntegrationTransaction)
            .WithMany(t => t.IntegrationTransactionRecords)
            .HasForeignKey(p => p.IntegrationTransactionId)
            .HasConstraintName("fk_integrationtransactionrecord_integrationtransaction");
    }
}

