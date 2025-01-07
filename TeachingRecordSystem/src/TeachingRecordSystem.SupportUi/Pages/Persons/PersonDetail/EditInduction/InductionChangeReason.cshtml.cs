using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class InductionChangeReasonModel : CommonJourneyPage
{
    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;
    public InductionJourneyPage NextPage => InductionJourneyPage.CheckAnswers;
    public string BackLink
    {
        get
        {
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresCompletedDate() => PageLink(InductionJourneyPage.CompletedDate),
                _ when InductionStatus.RequiresStartDate() => PageLink(InductionJourneyPage.StartDate),
                _ when InductionStatus.RequiresExemptionReasons() => PageLink(InductionJourneyPage.ExemptionReason),
                _ => PageLink(InductionJourneyPage.Status),
            };
        }
    }

    public InductionChangeReasonModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public Task OnGetAsync()
    {
        return JourneyInstance!.UpdateStateAsync(state =>
        {
            if (state.InductionStatus == InductionStatus.None)
            {
                state.InductionStatus = JourneyInstance!.State.CurrentInductionStatus;
            }
        });
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
