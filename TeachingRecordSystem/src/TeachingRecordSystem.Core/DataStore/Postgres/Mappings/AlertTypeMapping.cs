using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AlertTypeMapping : IEntityTypeConfiguration<AlertType>
{
    public void Configure(EntityTypeBuilder<AlertType> builder)
    {
        builder.ToTable("alert_types");
        builder.HasKey(x => x.AlertTypeId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AlertType.NameMaxLength).UseCollation("case_insensitive");
        builder.HasIndex(x => x.AlertCategoryId).HasDatabaseName(AlertType.AlertCategoryIdIndexName);
        builder.HasOne<AlertCategory>().WithMany().HasForeignKey(x => x.AlertCategoryId).HasConstraintName(AlertType.AlertCategoryForeignKeyName);
    }
}
