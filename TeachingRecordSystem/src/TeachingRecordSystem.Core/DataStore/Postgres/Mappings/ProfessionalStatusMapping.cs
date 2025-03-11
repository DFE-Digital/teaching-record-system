using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ProfessionalStatusMapping : IEntityTypeConfiguration<ProfessionalStatus>
{
    public void Configure(EntityTypeBuilder<ProfessionalStatus> builder)
    {
        builder.HasOne(q => q.Route).WithMany().HasForeignKey(q => q.RouteToProfessionalStatusId);
        builder.HasOne(q => q.TrainingCountry).WithMany().HasForeignKey(q => q.TrainingCountryId);
        builder.HasOne(q => q.TrainingProvider).WithMany().HasForeignKey(q => q.TrainingProviderId);
        builder.HasOne(q => q.InductionExemptionReason).WithMany().HasForeignKey(q => q.InductionExemptionReasonId);
        builder.HasOne(q => q.DegreeType).WithMany().HasForeignKey(q => q.DegreeTypeId);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(q => q.SourceApplicationUserId);
        builder
            .Property(q => q.SourceApplicationReference)
            .HasMaxLength(ProfessionalStatus.SourceApplicationReferenceMaxLength);
        builder.HasIndex(q => new { q.SourceApplicationUserId, q.SourceApplicationReference })
            .IsUnique()
            .HasFilter("source_application_user_id is not null and source_application_reference is not null");
    }
}
