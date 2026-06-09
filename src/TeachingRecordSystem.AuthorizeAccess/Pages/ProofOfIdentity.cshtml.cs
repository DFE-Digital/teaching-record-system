using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
[EnableRequestBuffering]
public class ProofOfIdentity(SignInJourneyCoordinator coordinator, ISafeFileService fileService) : PageModel
{
    public const int MaxFileSizeMb = 10;

    private readonly InlineValidator<ProofOfIdentity> _validator = new()
    {
        v => v.RuleFor(m => m.File)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Select a file")
            .MaxFileSize(MaxFileSizeMb)
            .WithMessage($"The file must be no larger than {MaxFileSizeMb}MB")
            .PermittedFileType(["image/jpeg", "image/png", "application/pdf"])
            .WithMessage("The file must be a PDF, JPG, or PNG")
    };

    [BindProperty]
    public new IFormFile? File { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var validationContext = ValidationContext<ProofOfIdentity>.CreateWithOptions(this, options => options.ThrowOnFailures());
        await _validator.ValidateAsync(validationContext);

        await using var stream = File!.OpenReadStream();

        if (!await fileService.TrySafeUploadAsync(stream, validationContext.GetMimeType(), out var fileId))
        {
            ModelState.AddModelError(nameof(File), "The selected file contains a virus");
            return this.PageWithErrors();
        }

        coordinator.UpdateState(state => state.SetProofOfIdentityFile(fileId, File.FileName));

        return coordinator.AdvanceTo(links => links.CheckAnswers());
    }
}
