using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class PersonMapping : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.IncludeInAnalyticsSync(includeAllColumns: false);
        builder.ToTable("persons");
        builder.HasKey(p => p.PersonId);
        builder.Property(p => p.PersonId).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.HasQueryFilter(p => p.Status == PersonStatus.Active);
        builder.Property(p => p.PersonId).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.HasIndex(p => p.DqtContactId).HasFilter("dqt_contact_id is not null").IsUnique();
        builder.Property(p => p.DqtContactId);
        builder.HasIndex(p => p.MergedWithPersonId).HasFilter("merged_with_person_id is not null");
        builder.Property(p => p.MergedWithPersonId).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.HasIndex(p => p.Trn).HasFilter("trn is not null").IsUnique();
        builder.Property(p => p.Trn)
            .HasDefaultValueSql("fn_generate_trn()")
            .ValueGeneratedOnAdd()
            .HasMaxLength(Person.TrnExactLength)
            .IsFixedLength()
            .ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property(p => p.FirstName)
            .HasMaxLength(Person.FirstNameMaxLength)
            .UseCollation(Collations.CaseInsensitive)
            .ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property(p => p.MiddleName)
            .HasMaxLength(Person.MiddleNameMaxLength)
            .UseCollation(Collations.CaseInsensitive)
            .ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property(p => p.LastName)
            .HasMaxLength(Person.LastNameMaxLength)
            .UseCollation(Collations.CaseInsensitive)
            .ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property(p => p.EmailAddress)
            .HasMaxLength(Person.EmailAddressMaxLength)
            .UseCollation(Collations.CaseInsensitive)
            .ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property(p => p.NationalInsuranceNumber)
            .HasMaxLength(Person.NationalInsuranceNumberMaxLength)
            .IsFixedLength()
            .ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property(p => p.Gender).ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property(p => p.DqtFirstName).HasMaxLength(100).UseCollation(Collations.CaseInsensitive);
        builder.Property(p => p.DqtMiddleName).HasMaxLength(100).UseCollation(Collations.CaseInsensitive);
        builder.Property(p => p.DqtLastName).HasMaxLength(100).UseCollation(Collations.CaseInsensitive);
        builder.Property(p => p.InductionStatus)
            .IsRequired()
            .HasDefaultValue(InductionStatus.None)
            .ConfigureAnalyticsSync(included: true, hidden: false);
        builder.HasOne<InductionStatusInfo>().WithMany().HasForeignKey(p => p.InductionStatus);
        builder.Property(p => p.InductionExemptionReasonIds)
            .ConfigureAnalyticsSync(included: true, hidden: false)
            .IsRequired();
        builder.Property(p => p.InductionStatusWithoutExemption)
            .ConfigureAnalyticsSync(included: true, hidden: false)
            .IsRequired();
        builder.Property(p => p.InductionStartDate).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.InductionCompletedDate).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.InductionModifiedOn).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.QtsDate).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.QtlsStatus).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.EytsDate).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.HasEyps).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.PqtsDate).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.CreatedByTps).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.MergedWithPersonId).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.HasOne(p => p.MergedWithPerson).WithMany().HasForeignKey(p => p.MergedWithPersonId);
        builder.Property(p => p.SourceApplicationUserId).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.Property(p => p.SourceTrnRequestId).ConfigureAnalyticsSync(included: true, hidden: false);
        builder.HasOne<TrnRequestMetadata>().WithMany().HasForeignKey(p => new { p.SourceApplicationUserId, p.SourceTrnRequestId });
        builder.Property(p => p.DateOfDeath).ConfigureAnalyticsSync(included: true, hidden: true);
        builder.Property<string[]>("names").HasColumnType("varchar[]").UseCollation(Collations.CaseInsensitive);
        builder.Property<string[]>("last_names").HasColumnType("varchar[]").UseCollation(Collations.CaseInsensitive);
        builder.Property<string[]>("national_insurance_numbers").HasColumnType("varchar[]").UseCollation(Collations.CaseInsensitive);
        builder.HasIndex("Trn", "DateOfBirth", "EmailAddress", "names", "last_names", "national_insurance_numbers")
            .HasMethod("GIN")
            .UseCollation(Collations.CaseInsensitive)
            .IsCreatedConcurrently();
    }
}
