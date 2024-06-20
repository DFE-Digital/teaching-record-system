using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.DataAnnotations;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class IdentityModel(AuthorizeAccessLinkGenerator linkGenerator, IFileService fileService) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    public const int MaxFileSizeMb = 3;

    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(MaxFileSizeMb * 1024 * 1024, ErrorMessage = "The selected file must be smaller than 3MB")]
    public IFormFile? EvidenceFile { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task OnGet()
    {
        UploadedEvidenceFileUrl ??= JourneyInstance?.State.EvidenceFileId is not null ?
            await fileService.GetFileUrl(JourneyInstance.State.EvidenceFileId.Value, _fileUrlExpiresAfter) :
            null;
    }

    public async Task<IActionResult> OnPost()
    {
        if (EvidenceFileId is null && EvidenceFile is null)
        {
            ModelState.AddModelError(nameof(EvidenceFile), "Select a file");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        if (EvidenceFile is not null)
        {
            if (EvidenceFileId is not null)
            {
                await fileService.DeleteFile(EvidenceFileId.Value);
            }

            using var stream = EvidenceFile.OpenReadStream();
            var evidenceFileId = await fileService.UploadFile(stream, EvidenceFile.ContentType);
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.EvidenceFileId = evidenceFileId;
                state.EvidenceFileName = EvidenceFile.FileName;
                state.EvidenceFileSizeDescription = EvidenceFile.Length.Bytes().Humanize();
            });
        }

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnNationalInsuranceNumber(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.DateOfBirth is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnDateOfBirth(JourneyInstance.InstanceId));
        }

        if (context.Result is null)
        {
            EvidenceFileId ??= JourneyInstance?.State.EvidenceFileId;
            EvidenceFileName ??= JourneyInstance?.State.EvidenceFileName;
            EvidenceFileSizeDescription ??= JourneyInstance?.State.EvidenceFileSizeDescription;
        }
    }
}
