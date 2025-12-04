using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    IFileService fileService) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.CanIdentityBeVerified)
            .NotNull().WithMessage("Select yes if you can verify this person’s identity")
    };

    public JourneyInstance<ResolveOneLoginUserIdVerificationState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string? SupportTaskReference { get; set; }

    [BindProperty]
    public bool? CanIdentityBeVerified { get; set; }

    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }
    public EvidenceInfo? Evidence { get; set; }

    public void OnGet()
    {
        CanIdentityBeVerified = JourneyInstance?.State.CanIdentityBeVerified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        await JourneyInstance!.UpdateStateAsync(state => state.CanIdentityBeVerified = CanIdentityBeVerified);

        return Redirect(CanIdentityBeVerified == true ?
            linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId) :
            linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Reject(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.Index());
    }

    public async Task<IActionResult> OnGetEvidenceAsync()
    {
        var stream = await fileService.OpenReadStreamAsync(Evidence!.FileId);
        return File(stream, Evidence.MimeType);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var data = supportTask.GetData<OneLoginUserIdVerificationData>();
        var oneLoginUser = await dbContext.OneLoginUsers
            .SingleOrDefaultAsync(u => u.Subject == data.OneLoginUserSubject);

        Name = StringHelper.JoinNonEmpty(' ', data.StatedFirstName, data.StatedLastName);
        DateOfBirth = data.StatedDateOfBirth;
        NationalInsuranceNumber = data.StatedNationalInsuranceNumber;
        Trn = data.StatedTrn;

        EmailAddress = oneLoginUser!.EmailAddress;

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
