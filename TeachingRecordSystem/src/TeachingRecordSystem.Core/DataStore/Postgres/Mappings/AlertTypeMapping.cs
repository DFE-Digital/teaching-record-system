using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AlertTypeMapping : IEntityTypeConfiguration<AlertType>
{
    public void Configure(EntityTypeBuilder<AlertType> builder)
    {
        builder.ToTable("alert_types");
        builder.HasKey(x => x.AlertTypeId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AlertType.NameMaxLength).UseCollation(Collations.CaseInsensitive);
        builder.Property(x => x.DqtSanctionCode).HasMaxLength(AlertType.DqtSanctionCodeMaxLength).UseCollation(Collations.CaseInsensitive);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Ignore(x => x.IsDbsAlertType);
        builder.HasIndex(x => x.AlertCategoryId).HasDatabaseName(AlertType.AlertCategoryIdIndexName);
        builder.HasIndex(x => new { x.AlertCategoryId, x.DisplayOrder }).HasDatabaseName(AlertType.DisplayOrderIndexName).IsUnique().HasFilter("display_order is not null and is_active = true");
        builder.HasOne(x => x.AlertCategory).WithMany(c => c.AlertTypes).HasForeignKey(x => x.AlertCategoryId).HasConstraintName(AlertType.AlertCategoryForeignKeyName);
        builder.Navigation(x => x.AlertCategory).AutoInclude();
    }
}
