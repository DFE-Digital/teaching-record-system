using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class ExemptionReasonModel : CommonJourneyPage
{
    public InductionJourneyPage NextPage => InductionJourneyPage.ChangeReasons;
    public string BackLink
    {
        // TODO - more logic needed when other routes to exemption reason are added
        get => PageLink(InductionJourneyPage.Status);
    }

    public ExemptionReasonModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
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
}
