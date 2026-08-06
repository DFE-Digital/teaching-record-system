using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.InductionExemptions;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), StartsJourney]
public class ExemptionReasonsModel(
    EditInductionJourneyCoordinator journey,
    InductionExemptionService inductionExemptionService) : PageModel
{
    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid[] ExemptionReasonIds { get; set; } = [];

    public Dictionary<ExemptionReasonCategory, IEnumerable<InductionExemptionReason>> ExemptionReasons { get; set; } = new();

    protected IEnumerable<RouteWithExemption>? RoutesWithInductionExemptions { get; private set; }

    public bool ShowInductionExemptionReasonNotAvailableMessage =>
        RoutesWithInductionExemptions?
            .Any(r => InductionExemptionService.ExemptionsToBeExcludedIfRouteQualificationIsHeld.Contains(r.InductionExemptionReasonId)) ?? false;

    public string[]? InductionExemptionFromRoutesMessages =>
        RoutesWithInductionExemptions is null || !RoutesWithInductionExemptions.Any()
            ? null
            : RoutesWithInductionExemptions
                .Select(r => $"This person has an induction exemption \"{r.InductionExemptionReasonName}\" on the \"{r.RouteToProfessionalStatusName}\" route.")
                .ToArray();

    public string[]? InductionExemptionReasonNotAvailableMessages =>
        !ShowInductionExemptionReasonNotAvailableMessage
            ? null
            : RoutesWithInductionExemptions!
                .Where(r => InductionExemptionService.ExemptionsToBeExcludedIfRouteQualificationIsHeld.Contains(r.InductionExemptionReasonId))
                .Select(r => $"To add/remove the induction exemption reason of: \"{r.InductionExemptionReasonName}\" please modify the \"{r.RouteToProfessionalStatusName}\" route.")
                .ToArray();

    public void OnGet()
    {
        ExemptionReasonIds = journey.State.ExemptionReasonIds;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        if (ExemptionReasonIds.Length == 0)
        {
            ModelState.AddModelError(nameof(ExemptionReasonIds), "Select the reason for a teacher’s exemption to induction");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        journey.UpdateState(state => state.ExemptionReasonIds = ExemptionReasonIds);

        return Redirect(journey.ContinueTo(journey.ReasonUrl()));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // Reachable as the journey's first step for a person whose status doesn't ask this question,
        // since a request can name the page directly. Once the journey is under way a status that
        // stops asking it truncates the path, so path validation is what turns the user away.
        if (journey.Status != InductionStatus.Exempt)
        {
            context.Result = Redirect(journey.InductionUrl);
            return;
        }

        BackLink = journey.GetBackLink() ?? journey.InductionUrl;

        var response = await inductionExemptionService.GetExemptionReasonsAsync(journey.PersonId);
        RoutesWithInductionExemptions = response.RoutesWithInductionExemptions;
        ExemptionReasons = response.ExemptionReasonCategories;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
