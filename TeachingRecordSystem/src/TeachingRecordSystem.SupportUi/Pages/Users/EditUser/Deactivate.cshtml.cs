using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Users.EditUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class DeactivateModel(
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    TrsDbContext dbContext,
    IClock clock) : PageModel
{
    private Core.DataStore.Postgres.Models.User? _user;

    [FromRoute]
    public Guid UserId { get; set; }

    [BindProperty]
    [Display(Name = "Reason for deactivating user")]
    [Required(ErrorMessage = "Select a reason for deactivating this user")]
    public bool? HasAdditionalReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter a reason for deactivating this user")]
    public string? AdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you have more information?")]
    [Required(ErrorMessage = "Select yes if you want to provide more details")]
    public bool? HasMoreInformation { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    public string? MoreInformationDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you have evidence to upload?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    [BindProperty]
    public Guid? EvidenceFileId { get; set; }

    [BindProperty]
    public string? EvidenceFileName { get; set; }

    [BindProperty]
    public string? EvidenceFileSizeDescription { get; set; }

    [BindProperty]
    public string? UploadedEvidenceFileUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_user!.Active)
        {
            return BadRequest();
        }

        // Only admins can deactivate admins
        if (!User.IsInRole(UserRoles.Administrator) && _user.Role == UserRoles.Administrator)
        {
            return BadRequest();
        }

        if (HasAdditionalReason == true && AdditionalReasonDetail is null)
        {
            ModelState.AddModelError(nameof(AdditionalReasonDetail), "Enter a reason");
        }

        if (HasMoreInformation == true && MoreInformationDetail is null)
        {
            ModelState.AddModelError(nameof(MoreInformationDetail), "Enter more details");
        }

        if (UploadEvidence == true && EvidenceFileId is null && EvidenceFile is null)
        {
            ModelState.AddModelError(nameof(EvidenceFile), "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (EvidenceFileId.HasValue && (EvidenceFile is not null || UploadEvidence != true))
        {
            await fileService.DeleteFileAsync(EvidenceFileId.Value);
        }

        // Upload the file even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (UploadEvidence == true)
        {
            // Upload the file and set the display fields
            if (EvidenceFile is not null)
            {
                using var stream = EvidenceFile.OpenReadStream();
                var fileId = await fileService.UploadFileAsync(stream, EvidenceFile.ContentType);
                EvidenceFileName = EvidenceFile?.FileName;
                EvidenceFileSizeDescription = EvidenceFile?.Length.Bytes().Humanize();
                UploadedEvidenceFileUrl = await fileService.GetFileUrlAsync(fileId, FileUploadDefaults.FileUrlExpiry);
                EvidenceFileId = fileId;
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        _user.Active = false;

        await dbContext.AddEventAndBroadcastAsync(new UserDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = EventModels.User.FromModel(_user),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow,
            DeactivatedReason = HasAdditionalReason is true ? AdditionalReasonDetail : null,
            DeactivatedReasonDetail = HasMoreInformation is true ? MoreInformationDetail : null,
            EvidenceFileId = EvidenceFileId,
        });

        await dbContext.SaveChangesAsync();
        TempData.SetFlashSuccess(messageText: $"{_user.Name}\u2019s account has been deactivated.");

        return Redirect(linkGenerator.Users());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await dbContext.Users.SingleOrDefaultAsync(u => u.UserId == UserId);

        if (_user is null)
        {
            context.Result = NotFound();
            return;
        }

        await next();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        // If the user cancels after having uploaded a file but before submitting,
        // we want to delete the file to clean up after ourselves.
        if (EvidenceFileId.HasValue)
        {
            await fileService.DeleteFileAsync(EvidenceFileId.Value);
        }

        return Redirect(linkGenerator.EditUser(UserId));
    }
}
