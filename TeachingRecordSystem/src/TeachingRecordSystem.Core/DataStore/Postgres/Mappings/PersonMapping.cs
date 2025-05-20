using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class PersonMapping : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");
        builder.HasKey(p => p.PersonId);
        builder.HasQueryFilter(p => p.Status == PersonStatus.Active);
        builder.HasIndex(p => p.DqtContactId).HasFilter("dqt_contact_id is not null").IsUnique();
        builder.HasIndex(p => p.MergedWithPersonId).HasFilter("merged_with_person_id is not null");
        builder.HasIndex(p => p.Trn).HasFilter("trn is not null").IsUnique();
        builder.Property(p => p.Trn).HasMaxLength(7).IsFixedLength();
        builder.Property(p => p.FirstName).HasMaxLength(Person.FirstNameMaxLength).UseCollation("case_insensitive");
        builder.Property(p => p.MiddleName).HasMaxLength(Person.MiddleNameMaxLength).UseCollation("case_insensitive");
        builder.Property(p => p.LastName).HasMaxLength(Person.LastNameMaxLength).UseCollation("case_insensitive");
        builder.Property(p => p.EmailAddress).HasMaxLength(Person.EmailAddressMaxLength).UseCollation("case_insensitive");
        builder.Property(p => p.MobileNumber).HasMaxLength(Person.MobileNumberMaxLength);
        builder.Property(p => p.NationalInsuranceNumber).HasMaxLength(Person.NationalInsuranceNumberMaxLength).IsFixedLength();
        builder.Property(p => p.DqtFirstName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.DqtMiddleName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.DqtLastName).HasMaxLength(100).UseCollation("case_insensitive");
        builder.Property(p => p.InductionStatus).IsRequired().HasDefaultValue(InductionStatus.None);
        builder.Property(p => p.InductionExemptionReasonIds).IsRequired();
        builder.Property(p => p.InductionStatusWithoutExemption).IsRequired();
        builder.HasOne(p => p.MergedWithPerson).WithMany().HasForeignKey(p => p.MergedWithPersonId);
    }
}
