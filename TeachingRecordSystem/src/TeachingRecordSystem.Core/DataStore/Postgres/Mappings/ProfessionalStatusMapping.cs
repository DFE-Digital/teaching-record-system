using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ProfessionalStatusMapping : IEntityTypeConfiguration<ProfessionalStatus>
{
    public void Configure(EntityTypeBuilder<ProfessionalStatus> builder)
    {
        builder.HasOne(q => q.Route).WithMany().HasForeignKey(q => q.RouteToProfessionalStatusId);
        builder.HasOne(q => q.Country).WithMany().HasForeignKey(q => q.CountryId);
        builder.HasOne(q => q.TrainingProvider).WithMany().HasForeignKey(q => q.TrainingProviderId);
        builder.HasOne(q => q.InductionExemptionReason).WithMany().HasForeignKey(q => q.InductionExemptionReasonId);
    }
}
