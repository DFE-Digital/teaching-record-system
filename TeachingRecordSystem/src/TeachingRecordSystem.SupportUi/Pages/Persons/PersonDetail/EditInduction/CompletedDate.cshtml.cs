using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class CompletedDateModel : CommonJourneyPage
{
    public InductionJourneyPage NextPage => InductionJourneyPage.ChangeReasons;
    public string BackLink
    {
        // TODO - more logic needed when other routes to completed-date are added
        get => PageLink(InductionJourneyPage.StartDate);
    }

    public CompletedDateModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the completed date
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.CompletedDate;
            }
        });

        return Redirect(PageLink(NextPage));
    }
}
