using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class CompletedDateModel : CommonJourneyPage
{
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

        return Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => _linkGenerator.InductionChangeReason(Id, journeyInstanceId);
    }
}
