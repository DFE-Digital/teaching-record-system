using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(IFileService fileService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Verified)
            .NotNull().WithMessage("Select yes if you can verify this person’s identity")
    };

    public JourneyInstance<ResolveOneLoginUserIdVerificationState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string? SupportTaskReference { get; set; }

    [BindProperty]
    public bool? Verified { get; set; }

    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }
    public EvidenceInfo? Evidence { get; set; }

    public void OnGet()
    {
        Verified = JourneyInstance?.State.Verified;
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance!.DeleteAsync();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
        }

        await _validator.ValidateAndThrowAsync(this);

        await JourneyInstance!.UpdateStateAsync(state => state.Verified = Verified);

        return Redirect(Verified is false ?
            linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Reject(SupportTaskReference!, JourneyInstance!.InstanceId) :
            JourneyInstance.State.MatchedPersons.Count > 0 ?
            linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId) :
            linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.NoMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnGetEvidenceAsync()
    {
        var stream = await fileService.OpenReadStreamAsync(Evidence!.FileId);
        return File(stream, Evidence.MimeType);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var oneLoginUser = supportTask.OneLoginUser!;
        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        Name = data.StatedFirstName + " " + data.StatedLastName;
        DateOfBirth = data.StatedDateOfBirth;
        NationalInsuranceNumber = data.StatedNationalInsuranceNumber;
        Trn = data.StatedTrn;
        EmailAddress = oneLoginUser.EmailAddress;

        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        if (!fileExtensionContentTypeProvider.TryGetContentType(data.EvidenceFileName, out var evidenceFileMimeType))
        {
            evidenceFileMimeType = "application/octet-stream";
        }

        Evidence = new EvidenceInfo()
        {
            FileId = data.EvidenceFileId,
            FileName = data.EvidenceFileName,
            FileUrl = await fileService.GetFileUrlAsync(data.EvidenceFileId, UiDefaults.FileUrlExpiry),
            MimeType = evidenceFileMimeType
        };

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record EvidenceInfo
    {
        public required Guid FileId { get; init; }
        public required string FileName { get; init; }
        public required string FileUrl { get; init; }
        public required string MimeType { get; init; }
    }
}
