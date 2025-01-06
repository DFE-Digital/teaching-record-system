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

    public InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;
    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter an induction start date")]
    [Display(Name = "Start date")]  // https://github.com/gunndabad/govuk-frontend-aspnetcore/issues/282
    public DateOnly? StartDate { get; set; }

    public InductionJourneyPage NextPage
    {
        get
        {
            return InductionStatus.RequiresCompletedDate()
                ? InductionJourneyPage.CompletedDate
                : InductionJourneyPage.ChangeReasons;
        }
    }

    public string BackLink
    {
        // TODO - more logic needed when other routes to start-date are added
        get => PageLink(InductionJourneyPage.Status);
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

        return Redirect(PageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.Status);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        await next();
    }
}
