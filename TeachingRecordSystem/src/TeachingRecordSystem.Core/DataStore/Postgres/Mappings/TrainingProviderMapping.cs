using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrainingProviderMapping : IEntityTypeConfiguration<TrainingProvider>
{
    public void Configure(EntityTypeBuilder<TrainingProvider> builder)
    {
        builder.ToTable("training_providers");
        builder.HasKey(x => x.TrainingProviderId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasData(
            new TrainingProvider
            {
                TrainingProviderId = new("98BCF32F-9F84-4142-89A5-ACCB616153A2"),
                Name = "Test provider",
                IsActive = true
            });
    }
}
