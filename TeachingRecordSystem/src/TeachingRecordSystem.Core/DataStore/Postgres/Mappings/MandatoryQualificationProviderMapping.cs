using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class MandatoryQualificationProviderMapping : IEntityTypeConfiguration<MandatoryQualificationProvider>
{
    public void Configure(EntityTypeBuilder<MandatoryQualificationProvider> builder)
    {
        builder.ToTable("mandatory_qualification_providers");
        builder.HasKey(p => p.MandatoryQualificationProviderId);
        builder.Property(p => p.Name).HasMaxLength(200);

        builder.HasData(MandatoryQualificationProvider.All);
    }
}
