using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class ExemptionReasonModel : CommonJourneyPage
{
    protected TrsDbContext _dbContext;
    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;
    public string? PersonName { get; set; }
    public InductionJourneyPage NextPage => InductionJourneyPage.ChangeReasons;

    public string BackLink
    {
        get
        {
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.ExemptionReason
                ? LinkGenerator.PersonInduction(PersonId)
                : PageLink(InductionJourneyPage.Status);
        }
    }

    public ExemptionReasonModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext) : base(linkGenerator)
    {
        _dbContext = dbContext;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the exemption reason
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.ExemptionReason;
            }
        });

        return Redirect(PageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.ExemptionReason);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        await next();
    }
}
