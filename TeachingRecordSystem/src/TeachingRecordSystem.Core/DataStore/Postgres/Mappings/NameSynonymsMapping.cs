using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class NameSynonymsMapping : IEntityTypeConfiguration<NameSynonyms>
{
    public void Configure(EntityTypeBuilder<NameSynonyms> builder)
    {
        builder.ToTable("name_synonyms");
        builder.HasKey(e => e.NameSynonymsId);
        builder.Property(e => e.Name).HasMaxLength(NameSynonyms.NameMaxLength).IsRequired().UseCollation("case_insensitive");
        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName(NameSynonyms.NameSynonymsIndexName);
        builder.Property(e => e.Synonyms).IsRequired().UseCollation("case_insensitive");
    }
}
