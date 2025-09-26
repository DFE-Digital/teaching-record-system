using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AlertMapping : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");
        builder.HasKey(x => x.AlertId);
        builder.HasQueryFilter(q => EF.Property<DateTime?>(q, nameof(Alert.DeletedOn)) == null);
        builder.Property(x => x.AlertTypeId).IsRequired();
        builder.Property(x => x.PersonId).IsRequired();
        builder.Property(x => x.Details);
        builder.HasIndex(x => x.AlertTypeId).HasDatabaseName(Alert.AlertTypeIdIndexName);
        builder.HasOne(x => x.AlertType).WithMany().HasForeignKey(x => x.AlertTypeId).HasConstraintName(Alert.AlertTypeForeignKeyName);
        builder.Navigation(x => x.AlertType).AutoInclude();
        builder.HasIndex(x => x.PersonId).HasDatabaseName(Alert.PersonIdIndexName);
        builder.HasOne(x => x.Person).WithMany(p => p.Alerts).HasForeignKey(x => x.PersonId).HasConstraintName(Alert.PersonForeignKeyName);
        builder.Ignore(x => x.IsOpen);
    }
}
