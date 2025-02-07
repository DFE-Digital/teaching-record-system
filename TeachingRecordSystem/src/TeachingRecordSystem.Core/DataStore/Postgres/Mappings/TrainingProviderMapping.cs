using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrainingProviderMapping : IEntityTypeConfiguration<TrainingProvider>
{
    public void Configure(EntityTypeBuilder<TrainingProvider> builder)
    {
        builder.ToTable("training_providers");
        builder.HasKey(x => x.TrainingProviderId);
        builder.HasIndex(x => x.Ukprn).HasDatabaseName(TrainingProvider.UkprnIndexName).IsUnique();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Ukprn).IsRequired().HasMaxLength(8).IsFixedLength();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
