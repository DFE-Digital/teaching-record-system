using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class RouteToProfessionalStatusMapping : IEntityTypeConfiguration<RouteToProfessionalStatus>
{
    public void Configure(EntityTypeBuilder<RouteToProfessionalStatus> builder)
    {
        builder.HasOne(q => q.RouteToProfessionalStatusType).WithMany().HasForeignKey(q => q.RouteToProfessionalStatusTypeId);
        builder.Navigation(q => q.RouteToProfessionalStatusType).AutoInclude();
        builder.HasOne(q => q.TrainingCountry).WithMany().HasForeignKey(q => q.TrainingCountryId);
        builder.Navigation(q => q.TrainingCountry).AutoInclude();
        builder.HasOne(q => q.TrainingProvider).WithMany().HasForeignKey(q => q.TrainingProviderId);
        builder.Navigation(q => q.TrainingProvider).AutoInclude();
        builder.HasOne(q => q.DegreeType).WithMany().HasForeignKey(q => q.DegreeTypeId);
        builder.Navigation(q => q.DegreeType).AutoInclude();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(q => q.SourceApplicationUserId);
        builder
            .Property(q => q.SourceApplicationReference)
            .HasMaxLength(RouteToProfessionalStatus.SourceApplicationReferenceMaxLength);
        builder.HasIndex(q => new { q.SourceApplicationUserId, q.SourceApplicationReference })
            .IsUnique()
            .HasFilter("source_application_user_id is not null and source_application_reference is not null");
        builder.Property(q => q.ExemptFromInductionDueToQtsDate);
    }
}
