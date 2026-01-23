using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class DegreeTypeMapping : IEntityTypeConfiguration<DegreeType>
{
    public void Configure(EntityTypeBuilder<DegreeType> builder)
    {
        builder.ToTable("degree_types");
        builder.HasKey(x => x.DegreeTypeId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(DegreeType.NameMaxLength);
        builder.Property(x => x.IsActive).IsRequired();

    }
}
