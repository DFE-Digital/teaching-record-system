using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StartDateModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    private InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;

    public DateOnly? CompletedDate => JourneyInstance!.State.CompletedDate;

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
                if (InductionStatus.RequiresCompletedDate() && StartDate > CompletedDate)
                {
                    return GetPageLink(InductionJourneyPage.CompletedDate, JourneyFromCheckYourAnswersPage.CheckYourAnswersToStartDate);
                }
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return InductionStatus.RequiresCompletedDate()
                ? GetPageLink(InductionJourneyPage.CompletedDate)
                : GetPageLink(InductionJourneyPage.ChangeReasons);
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
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.StartDate
            ? LinkGenerator.PersonInduction(PersonId)
            : GetPageLink(InductionJourneyPage.Status);
        }
    }

    public void OnGet()
    {
        StartDate = JourneyInstance!.State.StartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (StartDate > clock.Today)
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
        });

        return Redirect(NextPage);
    }

    protected override InductionJourneyPage StartPage => InductionJourneyPage.StartDate;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (!JourneyInstance!.State.InductionStatus.RequiresStartDate())
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }
    }
}
