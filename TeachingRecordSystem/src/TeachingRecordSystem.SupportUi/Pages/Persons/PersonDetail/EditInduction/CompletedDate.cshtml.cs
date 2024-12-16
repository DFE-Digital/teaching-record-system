using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class CompletedDateModel : CommonJourneyPage
{
    public InductionJourneyPage NextPage => InductionJourneyPage.ChangeReason;

    public CompletedDateModel(TrsLinkGenerator linkGenerator) :base(linkGenerator)
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
            state.PageBreadcrumb = InductionJourneyPage.CompletedDate;
        });

        return Redirect(PageLink(NextPage));
    }
}
