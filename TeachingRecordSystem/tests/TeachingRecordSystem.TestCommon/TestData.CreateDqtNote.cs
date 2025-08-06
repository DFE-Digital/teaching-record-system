using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<Note> CreateNoteFromDqtAsync(
            Guid personId,
            string contentHtml,
            Guid createdByUserId,
            string? createdByUserName,
            string? originalFileName,
            Guid? fileName,
            DateTime? createdDate = null) =>
        WithDbContextAsync(async dbContext =>
    {
        var noteId = Guid.NewGuid();

        var note = new Note
        {
            NoteId = noteId,
            PersonId = personId,
            ContentHtml = contentHtml,
            CreatedByDqtUserName = createdByUserName ?? GenerateName(),
            CreatedByDqtUserId = createdByUserId,
            CreatedOn = createdDate ?? Clock.UtcNow,
            UpdatedByDqtUserId = null,
            UpdatedOn = null,
            UpdatedByDqtUserName = null,
            OriginalFileName = originalFileName,
            FileId = originalFileName is not null
                ? noteId
                : null,
            Content = null,
            CreatedByUserId = null
        };

        dbContext.Notes.Add(note);
        await dbContext.SaveChangesAsync();

        return note;
    });
}
