using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class PreviousNameMapping : IEntityTypeConfiguration<PreviousName>
{
    public void Configure(EntityTypeBuilder<PreviousName> builder)
    {
        builder.IncludeInAnalyticsSync(includeAllColumns: false);
        builder.ToTable("previous_names");
        builder.HasKey(p => p.PreviousNameId);
        builder.Property(p => p.PreviousNameId).ConfigureAnalyticsSync(hidden: false);
        builder.Property(p => p.PersonId).ConfigureAnalyticsSync(hidden: true);
        builder.Property(p => p.FirstName)
            .HasMaxLength(Person.FirstNameMaxLength)
            .UseCollation(Collations.CaseInsensitive)
            .ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(p => p.MiddleName)
            .HasMaxLength(Person.MiddleNameMaxLength)
            .UseCollation(Collations.CaseInsensitive)
            .ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(p => p.LastName)
            .HasMaxLength(Person.LastNameMaxLength)
            .UseCollation(Collations.CaseInsensitive)
            .ConfigureAnalyticsSync(policyTag: PolicyTagNames.SensitiveHidden);
        builder.Property(p => p.CreatedOn).ConfigureAnalyticsSync(hidden: false);
        builder.Property(p => p.DeletedOn).ConfigureAnalyticsSync(hidden: false);
        builder.HasIndex(x => x.PersonId).HasDatabaseName(PreviousName.PersonIdIndexName);
        builder.HasOne(x => x.Person).WithMany(p => p.PreviousNames).HasForeignKey(x => x.PersonId).HasConstraintName(PreviousName.PersonForeignKeyName);
    }
}
