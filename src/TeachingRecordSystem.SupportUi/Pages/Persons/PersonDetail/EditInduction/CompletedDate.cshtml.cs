using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), StartsJourney]
public class CompletedDateModel(EditInductionJourneyCoordinator journey, TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<CompletedDateModel> _validator = new()
    {
        v => v.RuleFor(m => m.CompletedDate)
            .NotNull().WithMessage("Enter an induction completed date")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Completed date")]
    public DateOnly? CompletedDate { get; set; }

    public void OnGet()
    {
        CompletedDate = journey.State.CompletedDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        if (CompletedDate > timeProvider.Today)
        {
            ModelState.AddModelError(nameof(CompletedDate), "The induction completed date cannot be in the future");
        }

        if (CompletedDate < journey.State.StartDate)
        {
            ModelState.AddModelError(nameof(CompletedDate), "The induction completed date cannot be before the induction start date");
        }

        await this.ThrowIfInvalidAsync(_validator);

        journey.UpdateState(state => state.CompletedDate = CompletedDate);

        return Redirect(journey.ContinueTo(journey.ReasonUrl()));
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // Reachable as the journey's first step for a person whose status doesn't ask this question,
        // since a request can name the page directly. Once the journey is under way a status that
        // stops asking it truncates the path, so path validation is what turns the user away.
        if (!journey.Status.RequiresCompletedDate())
        {
            context.Result = Redirect(journey.InductionUrl);
            return Task.CompletedTask;
        }

        // The completed date is validated against the start date, so it can't be answered first.
        if (journey.State.StartDate is null)
        {
            context.Result = Redirect(journey.JourneyStartUrl);
            return Task.CompletedTask;
        }

        BackLink = journey.GetBackLink() ?? journey.InductionUrl;

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
