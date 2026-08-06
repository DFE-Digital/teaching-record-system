using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), StartsJourney]
public class StartDateModel(EditInductionJourneyCoordinator journey, TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<StartDateModel> _validator = new()
    {
        v => v.RuleFor(m => m.StartDate)
            .NotNull().WithMessage("Enter an induction start date")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Start date")]
    public DateOnly? StartDate { get; set; }

    public void OnGet()
    {
        StartDate = journey.State.StartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        if (StartDate > timeProvider.Today)
        {
            ModelState.AddModelError(nameof(StartDate), "The induction start date cannot be in the future");
        }

        if (StartDate < Person.EarliestInductionStartDate)
        {
            ModelState.AddModelError(nameof(StartDate), $"The induction start date cannot be before {Person.EarliestInductionStartDate.ToString(WebConstants.DateDisplayFormat)}");
        }

        await this.ThrowIfInvalidAsync(_validator);

        journey.UpdateState(state => state.StartDate = StartDate);

        // A start date that now falls after the completed date means the completed date has to be
        // asked for again, even when the user came here from check answers to change this one answer.
        if (journey.Status.RequiresCompletedDate() &&
            journey.State.CompletedDate < StartDate &&
            journey.ReturnUrl is string returnUrl)
        {
            return Redirect(journey.AdvanceToQuestion(journey.CompletedDateUrl(returnUrl)));
        }

        return Redirect(journey.ContinueTo(journey.NextQuestionAfterStartDate()));
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // Reachable as the journey's first step for a person whose status doesn't ask this question,
        // since a request can name the page directly. Once the journey is under way a status that
        // stops asking it truncates the path, so path validation is what turns the user away.
        if (!journey.Status.RequiresStartDate())
        {
            context.Result = Redirect(journey.InductionUrl);
            return Task.CompletedTask;
        }

        BackLink = journey.GetBackLink() ?? journey.InductionUrl;

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
