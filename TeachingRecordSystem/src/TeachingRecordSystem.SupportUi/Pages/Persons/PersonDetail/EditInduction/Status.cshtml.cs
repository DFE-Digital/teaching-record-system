using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Services.InductionRouting;
using TeachingRecordSystem.SupportUi.Services.InductionWizardPageLogic;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel : CommonJourneyPage
{
    protected TrsDbContext _dbContext;

    [BindProperty]
    public InductionStatus InductionStatus { get; set; }

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

        // Determine where to go next and redirect to that page
        return Redirect(PageLink(NextPageInJourney(InductionStatus)));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId);

        await next();
    }

    private InductionJourneyPage NextPageInJourney(InductionStatus status)
    {
        if (status == InductionStatus.Exempt)
        {
            return InductionJourneyPage.ExemptionReason;
        }
        if (status == InductionStatus.RequiredToComplete)
        {
            return InductionJourneyPage.ChangeReason;
        }
        else
        {
            return InductionJourneyPage.StartDate;
        }
    }
}
