using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StartDateModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    public InductionStatus InductionStatus { get; set; }

    public string BackLink => BackPage(InductionStatus)(PersonId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid PersonId { get; set; }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public void OnPost()
    {
        // TODO - store the start date

        // TODO - figure out where to go next and redirect to that page
        //Redirect(linkGenerator.InductionEditStartDate(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> NextPage(InductionStatus status)
    {
        if (status == InductionStatus.InProgress)
        {
            return (Id, journeyInstanceId) => linkGenerator.InductionChangeReason(Id, journeyInstanceId);
        }
        else if (status.RequiresCompletedDate())
        {
            return (Id, journeyInstanceId) => linkGenerator.InductionEditCompletedDate(Id, journeyInstanceId);
        }
        else
        {
            return (Id, journeyInstanceId) => linkGenerator.InductionEditExemptionReason(Id, journeyInstanceId);
        }
    }

    private Func<Guid, JourneyInstanceId, string> BackPage(InductionStatus status)
    {
        // TODO - determine the correct back link - it's either Change status page, or Induction details
        return (Id, journeyInstanceId) => linkGenerator.PersonInduction(Id);
    }
}
