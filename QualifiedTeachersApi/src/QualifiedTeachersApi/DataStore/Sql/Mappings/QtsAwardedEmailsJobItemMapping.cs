using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Infrastructure.EntityFramework;

namespace QualifiedTeachersApi.DataStore.Sql.Mappings;

public class QtsAwardedEmailsJobItemMapping : IEntityTypeConfiguration<QtsAwardedEmailsJobItem>
{
    public void Configure(EntityTypeBuilder<QtsAwardedEmailsJobItem> builder)
    {
        builder.ToTable("qts_awarded_emails_job_items");
        builder.Property(i => i.QtsAwardedEmailsJobId).IsRequired();
        builder.Property(i => i.PersonId).IsRequired();
        builder.HasKey(i => new { i.QtsAwardedEmailsJobId, i.PersonId });
        builder.Property(i => i.Trn).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(i => i.EmailAddress).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Personalization).HasConversion<DictionaryValueConverter, DictionaryValueComparer>().IsRequired().HasColumnType("jsonb");
        builder.HasIndex(i => i.Personalization).HasMethod("gin");
        builder.HasOne(i => i.QtsAwardedEmailsJob).WithMany(j => j.JobItems).HasForeignKey(i => i.QtsAwardedEmailsJobId);
    }
}
