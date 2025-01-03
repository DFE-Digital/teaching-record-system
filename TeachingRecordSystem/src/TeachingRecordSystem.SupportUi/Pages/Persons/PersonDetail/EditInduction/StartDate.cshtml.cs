using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StartDateModel : CommonJourneyPage
{
    public InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;

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

    public StartDateModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the start date
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.StartDate;
            }
        });

        return Redirect(PageLink(NextPage));
    }
}
