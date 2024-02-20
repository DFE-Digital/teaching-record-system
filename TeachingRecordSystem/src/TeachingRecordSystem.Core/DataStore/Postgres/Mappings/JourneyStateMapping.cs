using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class JourneyStateMapping : IEntityTypeConfiguration<JourneyState>
{
    public void Configure(EntityTypeBuilder<JourneyState> builder)
    {
        builder.ToTable("journey_states");
        builder.HasKey(s => s.InstanceId);
        builder.Property(s => s.InstanceId).IsRequired().HasMaxLength(300);
        builder.Property(s => s.UserId).IsRequired().HasMaxLength(200);
        builder.Property(s => s.State).IsRequired();
        builder.Property(s => s.Created).IsRequired();
        builder.Property(s => s.Updated).IsRequired();
    }
}
