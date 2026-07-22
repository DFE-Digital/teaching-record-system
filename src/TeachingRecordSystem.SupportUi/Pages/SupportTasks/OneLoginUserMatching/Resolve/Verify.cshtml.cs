using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class VerifyModel(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    ISafeFileService safeFileService,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<VerifyModel> _validator = new()
    {
        v => v.RuleFor(m => m.Verified)
            .NotNull().WithMessage("Select yes if you can verify this person’s identity")
    };

    [FromRoute]
    public required string SupportTaskReference { get; set; }

    [BindProperty]
    public bool? Verified { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }
    public EvidenceInfo? Evidence { get; set; }

    public void OnGet()
    {
        Verified = journey.State.Verified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
        }

        await _validator.ValidateAndThrowAsync(this);

        var resolveLinkGenerator = linkGenerator.SupportTasks.OneLoginUserMatching.Resolve;

        var nextStepUrl = Verified is false ?
            resolveLinkGenerator.Reject(journey.InstanceId) :
            string.IsNullOrWhiteSpace(Trn) || journey.State.MatchedPersons.Count == 0 ?
            resolveLinkGenerator.NoMatches(journey.InstanceId) :
            resolveLinkGenerator.Matches(journey.InstanceId);

        return journey.AdvanceTo(nextStepUrl, state => state.Verified = Verified);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink() ?? linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification();

        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var oneLoginUser = supportTask.OneLoginUser!;
        var data = supportTask.GetData<OneLoginUserIdVerificationData>();
        Name = data.StatedFirstName + " " + data.StatedLastName;
        DateOfBirth = data.StatedDateOfBirth;
        NationalInsuranceNumber = Core.NationalInsuranceNumber.Normalize(data.StatedNationalInsuranceNumber);
        Trn = TrnHelper.NormalizeTrn(data.StatedTrn);
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
            FileUrl = await safeFileService.GetFileUrlAsync(data.EvidenceFileId, WebConstants.FileUrlExpiry),
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
