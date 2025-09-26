using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class PreviousNameMapping : IEntityTypeConfiguration<PreviousName>
{
    public void Configure(EntityTypeBuilder<PreviousName> builder)
    {
        builder.ToTable("previous_names");
        builder.HasKey(p => p.PreviousNameId);
        builder.Property(p => p.FirstName).HasMaxLength(Person.FirstNameMaxLength).UseCollation(Collations.CaseInsensitive);
        builder.Property(p => p.MiddleName).HasMaxLength(Person.MiddleNameMaxLength).UseCollation(Collations.CaseInsensitive);
        builder.Property(p => p.LastName).HasMaxLength(Person.LastNameMaxLength).UseCollation(Collations.CaseInsensitive);
        builder.HasIndex(x => x.PersonId).HasDatabaseName(PreviousName.PersonIdIndexName);
        builder.HasOne(x => x.Person).WithMany(p => p.PreviousNames).HasForeignKey(x => x.PersonId).HasConstraintName(PreviousName.PersonForeignKeyName);
    }
}
