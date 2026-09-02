using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Inductions;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction)]
public class CheckAnswersModel(
    EditInductionJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    TimeProvider timeProvider,
    InductionService inductionService) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    // The URL a change link brings the user back to once they've answered the question again.
    public string ReturnUrl { get; set; } = null!;

    public InductionStatus InductionStatus { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public InductionExemptionReason[] ExemptionReasons { get; set; } = [];

    public IEnumerable<string> SelectedExemptionReasonsValues =>
        journey.State.ExemptionReasonIds
            .Join(ExemptionReasons, id => id, reason => reason.InductionExemptionReasonId, (_, reason) => reason.Name)
            .OrderByDescending(name => name);

    public PersonInductionChangeReason ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    // A journey started at 'change start date' can't also change the status, so only the answers the
    // user came in to change — and the ones that change alongside them — offer a Change link.
    public bool ShowStatusChangeLink => journey.StartedAtStatus;

    public bool ShowStartDateChangeLink => (journey.StartedAtStatus || journey.StartedAtStartDate) && InductionStatus.RequiresStartDate();

    public bool ShowCompletedDateChangeLink => InductionStatus.RequiresCompletedDate();

    public bool ShowExemptionReasonsChangeLink => InductionStatus == InductionStatus.Exempt;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        // The start date question sends the user on to the completed date when the two fall out of
        // order, so this should be unreachable — it's kept because the cost of being wrong is an
        // invalid pair of dates written to the record.
        if (StartDate > CompletedDate)
        {
            return Redirect(journey.CompletedDateUrl(returnUrl: ReturnUrl));
        }

        await inductionService.SetInductionStatusAsync(
            new SetInductionStatusOptions
            {
                PersonId = journey.PersonId,
                Status = InductionStatus,
                StartDate = StartDate,
                CompletedDate = CompletedDate,
                ExemptionReasonIds = journey.State.ExemptionReasonIds
            },
            new ProcessContext(
                ProcessType.PersonInductionUpdating,
                timeProvider.UtcNow,
                User.GetUserId(),
                new ChangeReasonWithDetailsAndEvidence
                {
                    Reason = ChangeReason.GetDisplayName(),
                    Details = ChangeReasonDetail,
                    EvidenceFile = EvidenceFile?.ToEventModel(),
                    AdditionalInformation = AdditionalInformation
                }));

        journey.DeleteInstance();

        TempData.SetFlashNotificationBanner("Induction details have been updated");

        return Redirect(linkGenerator.Persons.PersonDetail.Induction(journey.PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // The journey has four ways in and this page offers Change links, so unlike a journey that can
        // only be walked front to back it can be reached before every answer is in.
        if (!journey.IsComplete)
        {
            context.Result = Redirect(journey.JourneyStartUrl);
            return;
        }

        ReturnUrl = journey.CheckAnswersUrl();
        BackLink = journey.GetBackLink();

        ExemptionReasons = await referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);
        InductionStatus = journey.State.InductionStatus;
        StartDate = journey.State.StartDate;
        CompletedDate = journey.State.CompletedDate;
        ChangeReason = journey.State.ChangeReason!.Value;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;
        AdditionalInformation = journey.State.AdditionalInformation;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
