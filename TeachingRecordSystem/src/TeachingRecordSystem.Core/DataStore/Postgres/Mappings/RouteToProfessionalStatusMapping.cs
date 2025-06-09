using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class RouteToProfessionalStatusMapping : IEntityTypeConfiguration<RouteToProfessionalStatus>
{
    public void Configure(EntityTypeBuilder<RouteToProfessionalStatus> builder)
    {
        builder.HasOne(q => q.RouteToProfessionalStatusType).WithMany().HasForeignKey(q => q.RouteToProfessionalStatusTypeId);
        builder.HasOne(q => q.TrainingCountry).WithMany().HasForeignKey(q => q.TrainingCountryId);
        builder.HasOne(q => q.TrainingProvider).WithMany().HasForeignKey(q => q.TrainingProviderId);
        builder.HasOne(q => q.DegreeType).WithMany().HasForeignKey(q => q.DegreeTypeId);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(q => q.SourceApplicationUserId);
        builder
            .Property(q => q.SourceApplicationReference)
            .HasMaxLength(RouteToProfessionalStatus.SourceApplicationReferenceMaxLength);
        builder.HasIndex(q => new { q.SourceApplicationUserId, q.SourceApplicationReference })
            .IsUnique()
            .HasFilter("source_application_user_id is not null and source_application_reference is not null");
    }
}
