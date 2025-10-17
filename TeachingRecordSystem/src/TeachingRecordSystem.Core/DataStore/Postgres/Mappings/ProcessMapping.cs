using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class ProcessMapping : IEntityTypeConfiguration<Process>
{
    public void Configure(EntityTypeBuilder<Process> builder)
    {
        builder.ToTable("processes");
        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
        builder.HasIndex(a => a.ProcessType);
        builder.HasIndex(a => a.PersonIds).HasMethod("GIN");
    }
}
