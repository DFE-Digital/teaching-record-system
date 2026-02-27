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
    private static readonly string[] _evidenceFileExtensions =
        [".bmp", ".csv", ".doc", ".docx", ".eml", ".jpeg", ".jpg", ".mbox", ".msg", ".ods", ".odt", ".pdf", ".png", ".tif", ".txt", ".xls", ".xlsx"];

    private readonly InlineValidator<AddNote> _validator = new()
    {
        v => v.RuleFor(m => m.Content)
            .NotEmpty().WithMessage("Enter text for the note"),
        v => v.RuleFor(m => m.File)
            .Must(f => _evidenceFileExtensions.Any(e => f!.FileName.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX")
                .When(m => m.File is not null)
            .Must(f => f!.Length <= UiDefaults.MaxFileUploadSizeMb * 1024 * 1024)
                .WithMessage($"The selected file {UiDefaults.MaxFileUploadSizeErrorMessage}")
                .When(m => m.File is not null)
    };
    [FromRoute]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public new string? Content { get; set; }

    [BindProperty]
    public new IFormFile? File { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

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
