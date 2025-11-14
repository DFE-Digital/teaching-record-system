using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Notes;

public class NoteService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<Note> CreateNoteAsync(CreateNoteOptions options, ProcessContext processContext)
    {
        var note = new Note
        {
            NoteId = Guid.NewGuid(),
            PersonId = options.PersonId,
            Content = options.Content,
            CreatedOn = processContext.Now,
            UpdatedOn = processContext.Now,
            CreatedByUserId = options.CreatedByUserId,
            FileId = options.FileId,
            OriginalFileName = options.OriginalFileName
        };

        dbContext.Notes.Add(note);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new NoteCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = note.PersonId,
                Note = EventModels.Note.FromModel(note)
            },
            processContext);

        return note;
    }
}
