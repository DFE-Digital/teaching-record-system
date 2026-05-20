using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EntityChangesJournalMapping : IEntityTypeConfiguration<EntityChangesJournal>
{
    public void Configure(EntityTypeBuilder<EntityChangesJournal> builder)
    {
        builder.Property(p => p.Key).IsRequired();
        builder.Property(p => p.EntityLogicalName).IsRequired();
        builder.HasKey(p => new { p.Key, p.EntityLogicalName });
    }
}
