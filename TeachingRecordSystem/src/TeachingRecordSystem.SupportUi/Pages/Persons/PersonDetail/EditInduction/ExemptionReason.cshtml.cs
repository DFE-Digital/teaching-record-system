using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class ExemptionReasonModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    public string BackLink => BackPage()(PersonId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid PersonId { get; set; }

    public InductionStatus InductionStatus { get; set; }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public void OnPost()
    {
        // TODO - store the induction status

        // TODO - figure out where to go next and redirect to that page
        Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => linkGenerator.InductionChangeReason(Id, journeyInstanceId);
    }

    private Func<Guid, JourneyInstanceId, string> BackPage()
    {
        return (Id, journeyInstanceId) => linkGenerator.InductionEditStatus(Id, journeyInstanceId);
    }
}
