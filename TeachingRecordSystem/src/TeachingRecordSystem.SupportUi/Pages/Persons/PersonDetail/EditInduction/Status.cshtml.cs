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

    public InductionJourneyPage NextPageInJourney
    {
        get
        {
            return InductionStatus switch
            {
                InductionStatus.Exempt => InductionJourneyPage.ExemptionReason,
                _ when InductionStatus.RequiresStartDate() => InductionJourneyPage.StartDate,
                _ => InductionJourneyPage.ChangeReason
            };
        }
    }

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
            state.PageBreadcrumb = InductionJourneyPage.Status;
        });

        return Redirect(PageLink(NextPageInJourney));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId);

        await next();
    }

}
