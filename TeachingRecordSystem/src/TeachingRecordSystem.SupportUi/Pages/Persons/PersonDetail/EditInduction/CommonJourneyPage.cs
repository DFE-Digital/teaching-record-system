using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public abstract class CommonJourneyPage : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    protected TrsLinkGenerator _linkGenerator;

    [FromRoute]
    public Guid PersonId { get; set; }

    public string BackLink => BackPage(JourneyInstance!.State.PageBreadcrumb)(PersonId, JourneyInstance!.InstanceId);

    protected CommonJourneyPage(TrsLinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonInduction(PersonId));
    }

    protected Func<Guid, JourneyInstanceId, string> BackPage(EditInductionState.InductionJourneyPage? backPage)
    {
        return backPage switch
        {
            EditInductionState.InductionJourneyPage.Status => (Id, journeyInstanceId) => _linkGenerator.InductionEditStatus(Id, journeyInstanceId),
            EditInductionState.InductionJourneyPage.CompletedDate => (Id, journeyInstanceId) => _linkGenerator.InductionEditCompletedDate(Id, journeyInstanceId),
            EditInductionState.InductionJourneyPage.ExemptionReason => (Id, journeyInstanceId) => _linkGenerator.InductionEditExemptionReason(Id, journeyInstanceId),
            _ => (Id, journeyInstanceId) => _linkGenerator.PersonInduction(Id)
        };
    }
}
