using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class SupportTaskMapping : IEntityTypeConfiguration<SupportTask>
{
    public void Configure(EntityTypeBuilder<SupportTask> builder)
    {
        builder.ToTable("support_tasks");
        builder.HasKey(p => p.SupportTaskReference);
        builder.Property(p => p.SupportTaskReference).HasMaxLength(16);
        builder.HasOne<OneLoginUser>().WithMany().HasForeignKey(o => o.OneLoginUserSubject).HasConstraintName("fk_support_tasks_one_login_user");
        builder.HasOne<Person>().WithMany().HasForeignKey(p => p.PersonId).HasConstraintName("fk_support_tasks_person");
        builder.HasIndex(t => t.OneLoginUserSubject);
        builder.HasIndex(t => t.PersonId);
        builder.Property<JsonDocument>("_data").HasColumnName("data").IsRequired();
        builder.Ignore(t => t.Data);
    }
}
