using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class IntReturnMapping : IEntityTypeConfiguration<IntReturn>
{
    public void Configure(EntityTypeBuilder<IntReturn> builder)
    {
        builder.ToTable((string?)null);
        builder.HasNoKey();
    }
}
