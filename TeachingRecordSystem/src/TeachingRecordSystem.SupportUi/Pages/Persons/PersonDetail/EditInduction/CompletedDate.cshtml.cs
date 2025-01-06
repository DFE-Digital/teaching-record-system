using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class CompletedDateModel : CommonJourneyPage
{
    protected TrsDbContext _dbContext;
    protected IClock _clock;

    public InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;
    public string? PersonName { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Completed date")]
    [Required(ErrorMessage = "Enter an induction completed date")]
    [Display(Name = "When did they complete induction?")]
    public DateOnly? CompletedDate { get; set; }

    public InductionJourneyPage NextPage => InductionJourneyPage.ChangeReasons;

    public string BackLink
    {
        // TODO - more logic needed when other routes to completed-date are added
        get => PageLink(InductionJourneyPage.StartDate);
    }

    public CompletedDateModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext, IClock clock) : base(linkGenerator)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public void OnGet()
    {
        CompletedDate = JourneyInstance!.State.CompletedDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CompletedDate > _clock.Today)
        {
            ModelState.AddModelError(nameof(CompletedDate), "The induction completed date cannot be in the future");
        }
        if(CompletedDate < JourneyInstance!.State.StartDate)
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
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.CompletedDate;
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
