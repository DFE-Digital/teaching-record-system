using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class CompletedDateModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceUploadManager)
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Completed date")]
    [Required(ErrorMessage = "Enter an induction completed date")]
    public DateOnly? CompletedDate { get; set; }

    public InductionJourneyPage NextPage
    {
        get
        {
            if (FromCheckAnswers is JourneyFromCheckAnswersPage.CheckAnswers or JourneyFromCheckAnswersPage.CheckAnswersToStartDate)
            {
                return InductionJourneyPage.CheckAnswers;
            }
            return InductionJourneyPage.ChangeReasons;
        }
    }

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckAnswersPage.CheckAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            if (FromCheckAnswers == JourneyFromCheckAnswersPage.CheckAnswersToStartDate)
            {
                return GetPageLink(InductionJourneyPage.StartDate, JourneyFromCheckAnswersPage.CheckAnswers);
            }
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.CompletedDate
                ? LinkGenerator.Persons.PersonDetail.Induction(PersonId)
                : GetPageLink(InductionJourneyPage.StartDate);
        }
    }

    public void OnGet()
    {
        CompletedDate = JourneyInstance!.State.CompletedDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CompletedDate > clock.Today)
        {
            ModelState.AddModelError(nameof(CompletedDate), "The induction completed date cannot be in the future");
        }
        if (CompletedDate < JourneyInstance!.State.StartDate)
        {
            ModelState.AddModelError(nameof(CompletedDate), "The induction completed date cannot be before the induction start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.CompletedDate = CompletedDate;
        });

        return Redirect(GetPageLink(NextPage));
    }

    protected override InductionJourneyPage StartPage => InductionJourneyPage.CompletedDate;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (!JourneyInstance!.State.InductionStatus.RequiresCompletedDate() || !JourneyInstance!.State.StartDate.HasValue)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }
    }
}
