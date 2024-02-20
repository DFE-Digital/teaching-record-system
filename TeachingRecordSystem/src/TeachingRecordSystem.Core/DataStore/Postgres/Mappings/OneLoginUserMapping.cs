using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class OneLoginUserMapping : IEntityTypeConfiguration<OneLoginUser>
{
    public void Configure(EntityTypeBuilder<OneLoginUser> builder)
    {
        builder.HasKey(o => o.Subject);
        builder.Property(o => o.Subject).HasMaxLength(200);
        builder.Property(o => o.Email).HasMaxLength(200);
        builder.HasOne<Person>(o => o.Person).WithOne().HasForeignKey<OneLoginUser>(o => o.PersonId);
    }
}
