using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class InductionExemptionReasonMapping : IEntityTypeConfiguration<InductionExemptionReason>
{
    public void Configure(EntityTypeBuilder<InductionExemptionReason> builder)
    {
        builder.ToTable("induction_exemption_reasons");
        builder.Property(x => x.IsActive).IsRequired();

    }
}
