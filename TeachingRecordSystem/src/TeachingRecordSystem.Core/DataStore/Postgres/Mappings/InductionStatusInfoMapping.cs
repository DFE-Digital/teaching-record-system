using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class InductionStatusInfoMapping : IEntityTypeConfiguration<InductionStatusInfo>
{
    public void Configure(EntityTypeBuilder<InductionStatusInfo> builder)
    {
        builder.ToTable("induction_statuses");
        builder.HasKey(s => s.InductionStatus);
        builder.Property(s => s.Name).HasMaxLength(200);

        builder.HasData(InductionStatusRegistry.All.Select(i => new InductionStatusInfo { InductionStatus = i.InductionStatus, Name = i.Name }));
    }
}
