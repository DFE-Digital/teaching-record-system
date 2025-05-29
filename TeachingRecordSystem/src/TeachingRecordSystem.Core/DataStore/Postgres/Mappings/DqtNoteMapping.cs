using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class DqtNoteMapping : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("notes");
        builder.HasKey(x => x.NoteId);
        builder.Property(x => x.PersonId).IsRequired();
        builder.Property(x => x.ContentHtml).IsRequired(false);
        builder.Property(x => x.CreatedByDqtUserId).IsRequired(false);
        builder.Property(x => x.CreatedByDqtUserName).IsRequired(false);
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.HasOne<Person>().WithMany().HasForeignKey(q => q.PersonId);
    }
}
