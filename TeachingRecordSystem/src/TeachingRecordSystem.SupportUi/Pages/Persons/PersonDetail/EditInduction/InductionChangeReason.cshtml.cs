using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class InductionChangeReasonModel : CommonJourneyPage
{
    public InductionJourneyPage NextPage => InductionJourneyPage.CheckYourAnswers;
    public string BackLink
    {
        get
        {
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresCompletedDate() => PageLink(InductionJourneyPage.CompletedDate),
                _ when InductionStatus.RequiresStartDate() => PageLink(InductionJourneyPage.StartDate),
                _ when InductionStatus.RequiresExemptionReason() => PageLink(InductionJourneyPage.ExemptionReason),
                _ => PageLink(InductionJourneyPage.Status),
            };
        }
    }

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
        });

        return Redirect(PageLink(NextPage));
    }
}
