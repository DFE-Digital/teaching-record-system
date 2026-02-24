using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Users.EditUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class DeactivateModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    TrsDbContext dbContext,
    IClock clock) : PageModel
{
    private Core.DataStore.Postgres.Models.User? _user;

    [FromRoute]
    public Guid UserId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a reason for deactivating this user")]
    public bool? HasAdditionalReason { get; set; }

    [BindProperty]
    public string? AdditionalReasonDetail { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to provide more details")]
    public bool? HasMoreInformation { get; set; }

    [BindProperty]
    public string? MoreInformationDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

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

        await evidenceUploadManager.ValidateAndUploadAsync<DeactivateModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        _user.Active = false;

        dbContext.AddEventWithoutBroadcast(new UserDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = EventModels.User.FromModel(_user),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow,
            DeactivatedReason = HasAdditionalReason is true ? AdditionalReasonDetail : null,
            DeactivatedReasonDetail = HasMoreInformation is true ? MoreInformationDetail : null,
            EvidenceFileId = Evidence.UploadedEvidenceFile?.FileId
        });

        await dbContext.SaveChangesAsync();
        TempData.SetFlashSuccess($"{_user.Name}\u2019s account has been deactivated");

        return Redirect(linkGenerator.Users.Index());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(Evidence.UploadedEvidenceFile);
        return Redirect(linkGenerator.Users.EditUser.Index(UserId));
    }
}
