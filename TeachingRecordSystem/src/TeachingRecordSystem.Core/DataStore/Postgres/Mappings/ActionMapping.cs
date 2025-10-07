using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Action = TeachingRecordSystem.Core.DataStore.Postgres.Models.Action;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ActionMapping : IEntityTypeConfiguration<Action>
{
    public void Configure(EntityTypeBuilder<Action> builder)
    {
        builder.ToTable("actions");
        builder.HasIndex(a => a.PersonIds).HasMethod("GIN");
    }
}
