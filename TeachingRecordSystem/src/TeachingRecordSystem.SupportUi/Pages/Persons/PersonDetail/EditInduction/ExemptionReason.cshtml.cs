using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class ExemptionReasonModel : CommonJourneyPage
{
    public InductionJourneyPage NextPage => InductionJourneyPage.ChangeReason;

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
            state.PageBreadcrumb = InductionJourneyPage.ExemptionReason;
        });

        return Redirect(PageLink(NextPage));
    }
}
