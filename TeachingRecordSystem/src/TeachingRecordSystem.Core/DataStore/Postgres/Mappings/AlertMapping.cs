using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AlertMapping : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");
        builder.HasKey(x => x.AlertId);
        builder.Property(x => x.AlertTypeId).IsRequired();
        builder.Property(x => x.PersonId).IsRequired();
        builder.Property(x => x.Details).IsRequired();
        builder.HasIndex(x => x.AlertTypeId).HasDatabaseName(Alert.AlertTypeIdIndexName);
        builder.HasOne<AlertType>().WithMany().HasForeignKey(x => x.AlertTypeId).HasConstraintName(Alert.AlertTypeForeignKeyName);
        builder.HasIndex(x => x.PersonId).HasDatabaseName(Alert.PersonIdIndexName);
        builder.HasOne<Person>().WithMany().HasForeignKey(x => x.PersonId).HasConstraintName(Alert.PersonForeignKeyName);
    }
}
