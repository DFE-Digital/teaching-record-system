using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class VerifyModel(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    ISafeFileService safeFileService,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
    IFeatureProvider featureProvider) : PageModel
{
    public static class Actions
    {
        public const string SaveAndComeBackLater = nameof(SaveAndComeBackLater);
        public const string Cancel = nameof(Cancel);
    }

    private readonly InlineValidator<VerifyModel> _validator = new()
    {
        v => v.RuleFor(m => m.Verified)
            .NotNull().WithMessage("Select yes if you can verify this person’s identity")
    };

    private SupportTask? _supportTask;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    public bool? Verified { get; set; }

    public string? BackLink { get; set; }

    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }
    public EvidenceInfo? Evidence { get; set; }
    public string? QtsYearReceived { get; set; }
    public string? QtsProvider { get; set; }
    public string? QtsSubject { get; set; }
    public bool HasQtsDetails => !string.IsNullOrWhiteSpace(QtsYearReceived) || QtsProvider is not null || QtsSubject is not null;

    public void OnGet()
    {
        Verified = journey.State.Verified;
        journey.State.ApplySavedModelStateValues(nameof(VerifyModel), ModelState);
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            journey.DeleteInstance();

            return Redirect(journey.State.CompletionUrl);
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
        }

        await this.ThrowIfInvalidAsync(_validator);

        var resolveLinkGenerator = linkGenerator.SupportTasks.OneLoginUserMatching.Resolve;

        // if verification is false Move to reject screen
        // if there is only one definite match Move to confirm screen
        // if there is 0 matches Move to no matches screen
        // else move to matches screen
        string nextStepUrl;
        if (Verified is false)
        {
            nextStepUrl = resolveLinkGenerator.Reject(journey.InstanceId);
        }
        else if (journey.State.DefiniteMatch)
        {
            nextStepUrl = resolveLinkGenerator.ConfirmConnect(journey.InstanceId);
        }
        else if (string.IsNullOrWhiteSpace(Trn) || journey.State.MatchedPersons.Count == 0)
        {
            nextStepUrl = resolveLinkGenerator.NoMatches(journey.InstanceId);
        }
        else
        {
            nextStepUrl = resolveLinkGenerator.Matches(journey.InstanceId);
        }

        return journey.AdvanceTo(nextStepUrl, state =>
        {
            state.Verified = Verified;
            state.ClearSavedModelStateValues(nameof(VerifyModel));
            if (Verified is true && state.DefiniteMatch)
            {
                state.MatchedPersonId = journey.State.MatchedPersons.Single().PersonId;
            }
        });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(VerifyModel),
            journey.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processContext = new ProcessContext(
            ProcessType.OneLoginUserIdVerificationSupportTaskSaving,
            timeProvider.UtcNow,
            User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = _supportTask!.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        journey.DeleteInstance();

        if (featureProvider.IsEnabled("SupportTaskDashboard"))
        {
            return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(_supportTask.SupportTaskReference));
        }

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        BackLink = journey.GetBackLink() ?? journey.State.CompletionUrl;

        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var oneLoginUser = supportTask.OneLoginUser!;
        var data = supportTask.GetData<OneLoginUserIdVerificationData>();
        Name = data.StatedFirstName + " " + data.StatedLastName;
        DateOfBirth = data.StatedDateOfBirth;
        NationalInsuranceNumber = Core.NationalInsuranceNumber.Normalize(data.StatedNationalInsuranceNumber);
        Trn = TrnHelper.NormalizeTrn(data.StatedTrn);
        EmailAddress = oneLoginUser.EmailAddress;
        QtsYearReceived = data.YearQtsReceived;
        QtsProvider = data.TrainingProviderId is Guid trainingProviderId
            ? (await referenceDataCache.GetTrainingProviderByIdAsync(trainingProviderId)).Name
            : null;
        QtsSubject = data.SubjectId is Guid subjectId
            ? (await referenceDataCache.GetTrainingSubjectByIdAsync(subjectId)).Name
            : null;

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
