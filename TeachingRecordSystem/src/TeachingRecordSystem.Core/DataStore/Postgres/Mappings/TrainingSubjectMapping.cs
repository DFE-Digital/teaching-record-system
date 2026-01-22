using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TrainingSubjectMapping : IEntityTypeConfiguration<TrainingSubject>
{
    public void Configure(EntityTypeBuilder<TrainingSubject> builder)
    {
        builder.ToTable("training_subjects");
        builder.HasKey(x => x.TrainingSubjectId);
        builder.Property(x => x.Reference).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IsActive).IsRequired();

    }
}
