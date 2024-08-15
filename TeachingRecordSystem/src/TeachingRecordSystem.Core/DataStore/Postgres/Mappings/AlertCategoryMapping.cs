using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AlertCategoryMapping : IEntityTypeConfiguration<AlertCategory>
{
    public void Configure(EntityTypeBuilder<AlertCategory> builder)
    {
        builder.ToTable("alert_categories");
        builder.HasKey(x => x.AlertCategoryId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AlertCategory.NameMaxLength).UseCollation("case_insensitive");
    }
}
