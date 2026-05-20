using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class ProofOfIdentity(SignInJourneyCoordinator coordinator, ISafeFileService fileService) : PageModel
{
    public const int MaxFileSizeMb = 10;

    private readonly InlineValidator<ProofOfIdentity> _validator = new()
    {
        v => v.RuleFor(m => m.File)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Select a file")
            .Must((_, file) =>
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
                return allowedTypes.Contains(file!.ContentType);
            })
            .WithMessage("The file must be a PDF, JPG, or PNG")
            .Must((_, file) =>
            {
                var maxFileSizeInBytes = MaxFileSizeMb * 1024 * 1024; // 10 MB
                return file!.Length <= maxFileSizeInBytes;
            })
            .WithMessage($"The file must be no larger than {MaxFileSizeMb}MB")
    };

    [BindProperty]
    public new IFormFile? File { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        using var stream = File!.OpenReadStream();
        if (!await fileService.TrySafeUploadAsync(stream, File.ContentType, out var fileId))
        {
            ModelState.AddModelError(nameof(File), "The selected file contains a virus");
            return this.PageWithErrors();
        }

        coordinator.UpdateState(state => state.SetProofOfIdentityFile(fileId, File.FileName));

        return coordinator.AdvanceTo(links => links.CheckAnswers());
    }
}
