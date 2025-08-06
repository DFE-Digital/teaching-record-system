using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[AllowDeactivatedPerson]
public class NotesModel(TrsDbContext dbContext, IAuthorizationService authorizationService) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public IReadOnlyCollection<NoteViewModel>? Notes { get; set; }

    public bool CanViewNotes { get; set; }

    public async Task OnGetAsync()
    {
        CanViewNotes = (await authorizationService.AuthorizeAsync(User, PersonId, AuthorizationPolicies.NotesView)) is { Succeeded: true };

        var notesResult = await dbContext.Notes
            .Include(n => n.CreatedBy)
            .Where(n => n.PersonId == PersonId)
            .ToArrayAsync();

        Notes = await notesResult
            .ToAsyncEnumerable()
            .SelectAwait(async n => new NoteViewModel(
                n.NoteId,
                await n.GetNoteContentAsync(),
                n.CreatedOn.ToGmt(),
                n.FileId,
                n.OriginalFileName,
                n.CreatedByDqtUserName ?? n.CreatedBy?.Name
            ))
            .OrderByDescending(x => x.CreatedOn)
            .ToArrayAsync();
    }
}

public record NoteViewModel(Guid NoteId, string Content, DateTime CreatedOn, Guid? FileId, string? OriginalFileName, string? CreatedBy);
