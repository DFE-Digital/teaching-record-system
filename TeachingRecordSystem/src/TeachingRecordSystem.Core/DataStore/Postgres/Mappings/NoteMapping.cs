using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class NoteMapping : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasAnalyticsSync(columnsArePii: false);
        builder.ToTable("notes");
        builder.HasKey(x => x.NoteId);
        builder.Property(x => x.Content).HasAnalyticsSync(isPii: true);
        builder.Property(x => x.PersonId).IsRequired();
        builder.Property(x => x.ContentHtml).IsRequired(false);
        builder.Property(x => x.CreatedByDqtUserId).IsRequired(false);
        builder.Property(x => x.CreatedByDqtUserName).IsRequired(false);
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedByUserId);
        builder.HasOne<Person>().WithMany().HasForeignKey(q => q.PersonId);
        builder.HasIndex(x => x.PersonId).IsCreatedConcurrently();
    }
}
