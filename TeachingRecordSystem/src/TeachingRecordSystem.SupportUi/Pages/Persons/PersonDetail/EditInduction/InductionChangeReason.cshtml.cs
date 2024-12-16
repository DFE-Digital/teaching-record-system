using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class InductionChangeReasonModel : CommonJourneyPage
{
    public InductionJourneyPage NextPage => InductionJourneyPage.CheckYourAnswers;

    public InductionChangeReasonModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the change reason
            state.PageBreadcrumb = InductionJourneyPage.ChangeReason;
        });

        return Redirect(PageLink(NextPage));
    }
}
