using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class RouteToProfessionalStatusTypeMapping : IEntityTypeConfiguration<RouteToProfessionalStatusType>
{
    public void Configure(EntityTypeBuilder<RouteToProfessionalStatusType> builder)
    {
        builder.ToTable("route_to_professional_status_types");
        builder.Property(x => x.ProfessionalStatusType).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.TrainingStartDateRequired).IsRequired();
        builder.Property(x => x.TrainingStartDateRequired).IsRequired();
        builder.Property(x => x.HoldsFromRequired).IsRequired();
        builder.Property(x => x.InductionExemptionRequired).IsRequired();
        builder.Property(x => x.TrainingProviderRequired).IsRequired();
        builder.Property(x => x.DegreeTypeRequired).IsRequired();
        builder.Property(x => x.TrainingCountryRequired).IsRequired();
        builder.Property(x => x.TrainingAgeSpecialismTypeRequired).IsRequired();
        builder.Property(x => x.TrainingSubjectsRequired).IsRequired();
        builder.HasIndex(x => x.InductionExemptionReasonId).HasDatabaseName(RouteToProfessionalStatusType.InductionExemptionReasonIdIndexName);
        builder.HasOne(x => x.InductionExemptionReason).WithMany().HasForeignKey(x => x.InductionExemptionReasonId).HasConstraintName(RouteToProfessionalStatusType.InductionExemptionReasonForeignKeyName);
        builder.Navigation(x => x.InductionExemptionReason).AutoInclude();
    }
}
