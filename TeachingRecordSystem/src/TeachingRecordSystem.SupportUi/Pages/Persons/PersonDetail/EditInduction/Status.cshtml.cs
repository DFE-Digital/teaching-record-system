using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel : CommonJourneyPage
{
    protected TrsDbContext _dbContext;

    [BindProperty]
    public InductionStatus InductionStatus { get; set; }

    public InductionJourneyPage NextPage
    {
        get
        {
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresExemptionReasons() => InductionJourneyPage.ExemptionReason,
                _ when InductionStatus.RequiresStartDate() => InductionJourneyPage.StartDate,
                _ => InductionJourneyPage.ChangeReasons
            };
        }
    }
    public string BackLink => LinkGenerator.PersonInduction(PersonId);

    public StatusModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext) : base(linkGenerator)
    {
        _dbContext = dbContext;
    }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.InductionStatus = InductionStatus;
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.Status;
            }
        });

        return Redirect(PageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.Status);

        await next();
    }
}
