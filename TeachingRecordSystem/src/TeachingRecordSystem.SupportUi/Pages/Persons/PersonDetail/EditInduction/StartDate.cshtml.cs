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
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the start date
            state.PageBreadcrumb = EditInductionState.InductionJourneyPage.Status;
        });

        return Redirect(_linkGenerator.InductionEditStartDate(PersonId, JourneyInstance!.InstanceId)); //TODO add the other pages
    }

    private Func<Guid, JourneyInstanceId, string> NextPage(InductionStatus status)
    {
        if (status == InductionStatus.InProgress)
        {
            return (Id, journeyInstanceId) => _linkGenerator.InductionChangeReason(Id, journeyInstanceId);
        }
        else if (status.RequiresCompletedDate())
        {
            return (Id, journeyInstanceId) => _linkGenerator.InductionEditCompletedDate(Id, journeyInstanceId);
        }
        else
        {
            return (Id, journeyInstanceId) => _linkGenerator.InductionEditExemptionReason(Id, journeyInstanceId);
        }
    }
}
