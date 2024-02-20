using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class PersonSearchAttributeMapping : IEntityTypeConfiguration<PersonSearchAttribute>
{
    public void Configure(EntityTypeBuilder<PersonSearchAttribute> builder)
    {
        builder.ToTable("person_search_attributes");
        builder.HasKey(e => e.PersonSearchAttributeId);
        builder.Property(e => e.PersonId).IsRequired();
        builder.HasIndex(e => e.PersonId).HasDatabaseName(PersonSearchAttribute.PersonIdIndexName);
        builder.Property(e => e.AttributeType).HasMaxLength(PersonSearchAttribute.AttributeTypeMaxLength).IsRequired().UseCollation("case_insensitive");
        builder.Property(e => e.AttributeValue).HasMaxLength(PersonSearchAttribute.AttributeValueMaxLength).IsRequired().UseCollation("case_insensitive");
        builder.Property(e => e.AttributeKey).HasMaxLength(PersonSearchAttribute.AttributeKeyMaxLength).UseCollation("case_insensitive");
        builder.HasIndex(e => new { e.AttributeType, e.AttributeValue }).HasDatabaseName(PersonSearchAttribute.AttributeTypeAndValueIndexName);
    }
}
