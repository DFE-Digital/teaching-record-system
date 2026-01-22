using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class CountryMapping : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.CountryId);
        builder.Property(c => c.CountryId).HasMaxLength(10);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.OfficialName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CitizenNames).IsRequired().HasMaxLength(200);

    }
}
