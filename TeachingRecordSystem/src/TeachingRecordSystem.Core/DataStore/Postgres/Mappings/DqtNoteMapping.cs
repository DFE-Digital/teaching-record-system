using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class DqtNoteMapping : IEntityTypeConfiguration<DqtNote>
{
    public void Configure(EntityTypeBuilder<DqtNote> builder)
    {
        builder.ToTable("dqt_notes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PersonId).IsRequired();
        builder.Property(x => x.NoteText).IsRequired();
        builder.Property(x => x.CreatedByDqtUserId).IsRequired();
        builder.Property(x => x.CreatedByDqtUserName).IsRequired();
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.HasOne<Person>().WithMany().HasForeignKey(q => q.PersonId);
    }
}
