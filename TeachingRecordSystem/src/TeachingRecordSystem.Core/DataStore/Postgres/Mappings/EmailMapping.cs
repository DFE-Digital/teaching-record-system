using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Infrastructure.EntityFramework;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class EmailMapping : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.Property(e => e.TemplateId).HasMaxLength(Guid.Empty.ToString().Length);
        builder.Property(e => e.EmailAddress).HasMaxLength(200);
        builder.Property(e => e.Personalization).HasJsonConversion().IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.Metadata).HasJsonConversion().IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.EmailReplyToId).HasMaxLength(Guid.Empty.ToString().Length);
    }
}
