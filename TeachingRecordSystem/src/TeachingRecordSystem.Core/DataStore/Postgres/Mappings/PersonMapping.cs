using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class PersonMapping : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");
        builder.HasKey(p => p.PersonId);
        builder.HasIndex(p => p.DqtContactId).HasFilter("dqt_contact_id is not null").IsUnique();
        builder.HasIndex(p => p.Trn).HasFilter("trn is not null").IsUnique();
        builder.Property(p => p.Trn).HasMaxLength(7).IsFixedLength();
        builder.Property(p => p.FirstName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.MiddleName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.LastName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.EmailAddress).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.NationalInsuranceNumber).HasMaxLength(9).IsFixedLength();
        builder.Property(p => p.DqtFirstName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.DqtMiddleName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.DqtLastName).HasMaxLength(100).UseCollation("case_insensitive");
    }
}
