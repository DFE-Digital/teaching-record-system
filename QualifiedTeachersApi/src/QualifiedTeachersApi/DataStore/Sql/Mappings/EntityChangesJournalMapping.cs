using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QualifiedTeachersApi.DataStore.Sql.Models;

namespace QualifiedTeachersApi.DataStore.Sql.Mappings;

public class EntityChangesJournalMapping : IEntityTypeConfiguration<EntityChangesJournal>
{
    public void Configure(EntityTypeBuilder<EntityChangesJournal> builder)
    {
        builder.Property(p => p.Key).IsRequired();
        builder.Property(p => p.EntityLogicalName).IsRequired();
        builder.Property(p => p.DataToken).IsRequired();
        builder.HasKey(p => new { p.Key, p.EntityLogicalName });
    }
}
