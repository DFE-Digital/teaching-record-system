using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTaskNotes;

public class SupportTaskNoteService(TrsDbContext dbContext, TimeProvider timeProvider)
{
    public async Task<SupportTaskNote> CreateNoteAsync(CreateSupportTaskNoteOptions options)
    {
        var note = new SupportTaskNote
        {
            SupportTaskNoteId = Guid.NewGuid(),
            SupportTaskReference = options.SupportTaskReference,
            Content = options.Content,
            CreatedOn = timeProvider.GetUtcNow().UtcDateTime,
            CreatedByUserId = options.CreatedByUserId
        };

        dbContext.SupportTaskNotes.Add(note);
        await dbContext.SaveChangesAsync();

        return note;
    }
}
