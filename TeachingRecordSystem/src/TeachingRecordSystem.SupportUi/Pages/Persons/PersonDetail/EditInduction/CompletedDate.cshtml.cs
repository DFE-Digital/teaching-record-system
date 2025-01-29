using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class CompletedDateModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext, IClock clock)
    : CommonJourneyPage(linkGenerator)
{
    public string? PersonName { get; set; }

    [FromQuery]
    public JourneyFromCheckYourAnswersPage? FromCheckAnswers { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Completed date")]
    [Required(ErrorMessage = "Enter an induction completed date")]
    [Display(Name = "When did they complete induction?")]
    public DateOnly? CompletedDate { get; set; }

    public InductionJourneyPage NextPage
    {
        get
        {
            if (FromCheckAnswers is JourneyFromCheckYourAnswersPage.CheckYourAnswers or JourneyFromCheckYourAnswersPage.CheckYourAnswersToStartDate)
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
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswersToStartDate)
            {
                return GetPageLink(InductionJourneyPage.StartDate, JourneyFromCheckYourAnswersPage.CheckYourAnswers);
            }
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.CompletedDate
                ? LinkGenerator.PersonInduction(PersonId)
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

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(dbContext, PersonId, InductionJourneyPage.CompletedDate);

        if (!JourneyInstance!.State.InductionStatus.RequiresCompletedDate() || !JourneyInstance!.State.StartDate.HasValue)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        await next();
    }
}
