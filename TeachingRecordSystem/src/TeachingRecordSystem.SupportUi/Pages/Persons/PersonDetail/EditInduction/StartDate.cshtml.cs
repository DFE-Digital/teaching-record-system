using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StartDateModel : CommonJourneyPage
{
    protected TrsDbContext _dbContext;
    protected IClock _clock;

    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;
    public DateOnly? CompletedDate => JourneyInstance!.State.CompletedDate;
    public string? PersonName { get; set; }

    [FromQuery]
    public JourneyFromCheckYourAnswersPage? FromCheckAnswers { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Start date")]
    [Required(ErrorMessage = "Enter an induction start date")]
    [Display(Name = "When did they start induction?")]
    public DateOnly? StartDate { get; set; }

    public string NextPage
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                if ((InductionStatus.RequiresCompletedDate() && StartDate > CompletedDate) ||
                    (InductionStatus.RequiresCompletedDate() && StartDate > CompletedDate?.AddYears(-2)))
                {
                    return PageLink(InductionJourneyPage.CompletedDate, JourneyFromCheckYourAnswersPage.CheckYourAnswersToStartDate);
                }
                return PageLink(InductionJourneyPage.CheckAnswers);
            }
            return InductionStatus.RequiresCompletedDate()
                ? PageLink(InductionJourneyPage.CompletedDate)
                : PageLink(InductionJourneyPage.ChangeReasons);
        }
    }

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return PageLink(InductionJourneyPage.CheckAnswers);
            }
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.StartDate
            ? LinkGenerator.PersonInduction(PersonId)
            : PageLink(InductionJourneyPage.Status);
        }
    }

    public StartDateModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext, IClock clock) : base(linkGenerator)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public void OnGet()
    {
        StartDate = JourneyInstance!.State.StartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (StartDate > _clock.Today)
        {
            ModelState.AddModelError(nameof(StartDate), "The induction start date cannot be in the future");
        }
        if (StartDate < Person.EarliestInductionStartDate)
        {
            ModelState.AddModelError(nameof(StartDate), $"The induction start date cannot be before {Person.EarliestInductionStartDate.ToString(UiDefaults.DateOnlyDisplayFormat)}");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.StartDate = StartDate!.Value;
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.StartDate;
            }
        });

        return Redirect(NextPage);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.StartDate);
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        await next();
    }
}
