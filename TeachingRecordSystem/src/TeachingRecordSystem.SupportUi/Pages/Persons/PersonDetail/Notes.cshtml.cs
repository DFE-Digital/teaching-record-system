using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class NotesModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public List<Note> Notes { get; set; } = new List<Note>();

    public async Task OnGetAsync()
    {
        var notesResult = await dbContext.Notes
            .Where(x => x.PersonId == PersonId)
            .ToArrayAsync();

        var noteTasks = notesResult.Select(async x =>
            new Note(
                x.NoteId,
                string.Empty,
                await x.GetNoteTextWithoutHtmlAsync(),
                x.CreatedOn.ToLocal(),
                x.FileName,
                x.OriginalFileName,
                x.CreatedByDqtUserName
            )
        );
        var notesArray = await Task.WhenAll(noteTasks);

        Notes = notesArray
            .OrderByDescending(x => x.CreatedOn)
            .ToList();
    }
}

public record Note(Guid NoteId, string Title, string Description, DateTime CreatedOn, string? FileName, string? OriginalFileName, string? CreatedByDqtUserName);
