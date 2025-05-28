using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon
{
    public partial class TestData
    {
        public Task<Note> CreateNoteAsync(Guid personId, string noteText, Guid createdByUserId, string? createdByUserName, string? originalFileName, Guid? fileName, DateTime? createDate = null) => WithDbContextAsync(async dbContext =>
        {
            var note = new Note()
            {
                NoteId = Guid.NewGuid(),
                PersonId = personId,
                ContentHtml = noteText,
                CreatedByDqtUserName = createdByUserName ?? Faker.Name.FullName(),
                CreatedByDqtUserId = createdByUserId,
                CreatedOn = createDate ?? Clock.UtcNow,
                UpdatedByDqtUserId = null,
                UpdatedOn = null,
                UpdatedByDqtUserName = null,
                OriginalFileName = originalFileName,
                FileName = fileName?.ToString()
            };

            dbContext.Notes.Add(note);
            await dbContext.SaveChangesAsync();

            return note;
        });
    }
}
