using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class ExemptionReasonModel : CommonJourneyPage
{
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

        return Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => _linkGenerator.InductionChangeReason(Id, journeyInstanceId);
    }

}
