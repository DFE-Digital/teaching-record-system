using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class PersonEmploymentMapping : IEntityTypeConfiguration<PersonEmployment>
{
    public void Configure(EntityTypeBuilder<PersonEmployment> builder)
    {
        builder.ToTable("person_employments");
        builder.HasKey(e => e.PersonEmploymentId);
        builder.Property(e => e.EstablishmentId).IsRequired();
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.EmploymentType).IsRequired();
        builder.Property(e => e.CreatedOn).IsRequired();
        builder.Property(e => e.UpdatedOn).IsRequired();
        builder.HasOne<Person>().WithMany().HasForeignKey(e => e.PersonId).HasConstraintName("fk_person_employments_person_id");
        builder.HasOne<Establishment>().WithMany().HasForeignKey(e => e.EstablishmentId).HasConstraintName("fk_person_employments_establishment_id");
    }
}
