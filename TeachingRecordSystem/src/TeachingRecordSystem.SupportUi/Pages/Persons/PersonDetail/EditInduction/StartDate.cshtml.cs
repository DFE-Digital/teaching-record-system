using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StartDateModel : CommonJourneyPage
{
    public InductionStatus InductionStatus { get; set; }

    public InductionJourneyPage NextPage
    {
        get
        {
            return InductionStatus.RequiresCompletedDate()
                ? InductionJourneyPage.CompletedDate
                : InductionJourneyPage.ChangeReason;
        }
    }

    public StartDateModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the start date
            state.PageBreadcrumb = InductionJourneyPage.StartDate;
        });

        return Redirect(PageLink(NextPage));
    }
}
