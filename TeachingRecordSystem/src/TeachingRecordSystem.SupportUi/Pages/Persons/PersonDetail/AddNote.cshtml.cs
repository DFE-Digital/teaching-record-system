using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.Notes;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[Authorize(Policy = AuthorizationPolicies.PersonDataEdit)]
public class AddNote(NoteService noteService, SupportUiLinkGenerator linkGenerator, IFileService fileService, TimeProvider timeProvider) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter text for the note")]
    public new string? Content { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(UiDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {UiDefaults.MaxFileUploadSizeErrorMessage}")]
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

        var processContext = new ProcessContext(ProcessType.NoteCreating, timeProvider.UtcNow, User.GetUserId());

        await noteService.CreateNoteAsync(
            new CreateNoteOptions
            {
                PersonId = PersonId,
                Content = Content!,
                CreatedByUserId = User.GetUserId(),
                FileId = fileId,
                OriginalFileName = File?.FileName
            },
            processContext);

        return Redirect(linkGenerator.Persons.PersonDetail.Notes(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var currentPerson = HttpContext.GetCurrentPersonFeature();
        PersonName = currentPerson.Name;
    }
}
