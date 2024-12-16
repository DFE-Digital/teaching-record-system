using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StartDateModel : CommonJourneyPage
{
    public InductionStatus InductionStatus { get; set; }

    public StartDateModel(TrsLinkGenerator linkGenerator) :base(linkGenerator)
    {
    }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the start date
            state.PageBreadcrumb = InductionJourneyPage.StartDate;
        });

        return Redirect(NextPage(InductionStatus)(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> NextPage(InductionStatus status)
    {
        if (status.RequiresCompletedDate())
        {
            return (Id, journeyInstanceId) => _linkGenerator.InductionEditCompletedDate(Id, journeyInstanceId);
        }
        else
        {
            return (Id, journeyInstanceId) => _linkGenerator.InductionChangeReason(Id, journeyInstanceId);
        }
    }
}
