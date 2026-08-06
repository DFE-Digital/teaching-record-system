using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), StartsJourney]
public class StatusModel(
    EditInductionJourneyCoordinator journey,
    TrsDbContext dbContext,
    TimeProvider timeProvider) : PageModel
{
    private bool _inductionStatusManagedByCpd;

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    [NotEqual(InductionStatus.None, ErrorMessage = "Select a status")]
    public InductionStatus InductionStatus { get; set; }

    public InductionStatus CurrentInductionStatus { get; set; }

    public IEnumerable<InductionStatusDescription> StatusChoices =>
        _inductionStatusManagedByCpd && CurrentInductionStatus is not InductionStatus.FailedInWales and not InductionStatus.Exempt
            ? InductionStatusRegistry.ValidStatusChangesWhenManagedByCpd
                .Append(InductionStatusRegistry.All.Single(i => i.InductionStatus == CurrentInductionStatus))
                .OrderBy(i => i.InductionStatus)
                .ToArray()
            : InductionStatusRegistry.All.ToArray()[1..];

    public string InductionIsManagedByCpdWarning => CurrentInductionStatus switch
    {
        InductionStatus.RequiredToComplete => InductionWarnings.InductionIsManagedByCpdWarningRequiredToComplete,
        InductionStatus.InProgress => InductionWarnings.InductionIsManagedByCpdWarningInProgress,
        InductionStatus.Passed => InductionWarnings.InductionIsManagedByCpdWarningPassed,
        InductionStatus.Failed => InductionWarnings.InductionIsManagedByCpdWarningFailed,
        _ => InductionWarnings.InductionIsManagedByCpdWarningOther
    };

    public string? StatusWarningMessage =>
        _inductionStatusManagedByCpd && CurrentInductionStatus is not InductionStatus.FailedInWales and not InductionStatus.Exempt
            ? InductionIsManagedByCpdWarning
            : null;

    public void OnGet()
    {
        InductionStatus = journey.State.InductionStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        return Redirect(journey.AnswerStatusAndAdvance(state =>
        {
            state.InductionStatus = InductionStatus;

            // Drop the answers the new status doesn't ask for, so a journey that no longer shows them
            // can't carry them through to the change.
            if (!InductionStatus.RequiresStartDate())
            {
                state.StartDate = null;
            }

            if (!InductionStatus.RequiresCompletedDate())
            {
                state.CompletedDate = null;
            }

            if (!InductionStatus.RequiresExemptionReasons())
            {
                state.ExemptionReasonIds = [];
            }
        }));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink() ?? journey.InductionUrl;
        CurrentInductionStatus = journey.State.CurrentInductionStatus;

        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == journey.PersonId);
        _inductionStatusManagedByCpd = person.InductionStatusManagedByCpd(timeProvider.Today);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
