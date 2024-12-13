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
            state.PageBreadcrumb = EditInductionState.InductionJourneyPage.Status;
        });

        // Determine where to go next and redirect to that page
        string url = NextPage(InductionStatus)(PersonId, JourneyInstance!.InstanceId);
        return Redirect(url);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId);

        await next();
    }

    private Func<Guid, JourneyInstanceId, string> NextPage(InductionStatus status)
    {
        if (status == InductionStatus.Exempt)
        {
            return (Id, journeyInstanceId) => _linkGenerator.InductionEditExemptionReason(Id, journeyInstanceId);
        }
        //if (status == InductionStatus.RequiredToComplete)
        //{
        //    return (personId, journeyInstanceId) => linkGenerator.InductionChangeReason(personId, journeyInstanceId);
        //}
        else
        {
            return (Id, journeyInstanceId) => _linkGenerator.InductionEditStartDate(Id, journeyInstanceId);
        }
    }
}
