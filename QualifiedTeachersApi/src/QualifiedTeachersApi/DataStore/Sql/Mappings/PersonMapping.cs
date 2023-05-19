using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QualifiedTeachersApi.DataStore.Sql.Models;

namespace QualifiedTeachersApi.DataStore.Sql.Mappings;

public class PersonMapping : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("people");
        builder.Property(p => p.Trn).HasMaxLength(Person.TrnLength).IsFixedLength(true);
        builder.Property(p => p.FirstName).HasMaxLength(Person.FirstNameMaxLength).UseCollation("case_insensitive");
        builder.Property(p => p.MiddleName).HasMaxLength(Person.MiddleNameMaxLength).UseCollation("case_insensitive");
        builder.Property(p => p.LastName).HasMaxLength(Person.LastNameMaxLength).UseCollation("case_insensitive");
        builder.HasIndex(p => p.Trn).HasDatabaseName("ix_people_trn").IsUnique();
    }
}
