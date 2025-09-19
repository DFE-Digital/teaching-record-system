using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[Authorize(Policy = AuthorizationPolicies.PersonDataEdit)]
public class AddNote(
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter text for the note")]
    public string? Text { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public new IFormFile? File { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        Guid? fileId = null;
        if (File is not null)
        {
            await using var stream = File.OpenReadStream();
            fileId = await fileService.UploadFileAsync(stream, File.ContentType);
        }

        var now = clock.UtcNow;

        var note = new Core.DataStore.Postgres.Models.Note
        {
            NoteId = Guid.NewGuid(),
            PersonId = PersonId,
            Content = Text!,
            UpdatedOn = now,
            CreatedOn = now,
            CreatedByUserId = User.GetUserId(),
            FileId = fileId,
            OriginalFileName = File?.FileName
        };
        dbContext.Notes.Add(note);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(new NoteCreatedEvent
        {
            Note = EventModels.Note.FromModel(note),
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId()
        });

        return Redirect(linkGenerator.PersonNotes(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var currentPerson = HttpContext.GetCurrentPersonFeature();
        PersonName = currentPerson.Name;
    }
}
