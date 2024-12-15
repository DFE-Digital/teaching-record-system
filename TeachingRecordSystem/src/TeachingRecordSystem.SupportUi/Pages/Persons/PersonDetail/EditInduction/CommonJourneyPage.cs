using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;
using TeachingRecordSystem.SupportUi.Services.InductionWizardPageLogic;

public abstract class CommonJourneyPage : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    protected TrsLinkGenerator _linkGenerator;

    [FromRoute]
    public Guid PersonId { get; set; }

    public string BackLink => PageLink(JourneyInstance!.State.PageBreadcrumb);

    protected CommonJourneyPage(TrsLinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonInduction(PersonId));
    }

    protected string PageLink(InductionJourneyPage? pageName)
    {
        return pageName switch
        {
            InductionJourneyPage.Status => _linkGenerator.InductionEditStatus(PersonId, JourneyInstance!.InstanceId),
            InductionJourneyPage.CompletedDate => _linkGenerator.InductionEditCompletedDate(PersonId, JourneyInstance!.InstanceId),
            InductionJourneyPage.ExemptionReason => _linkGenerator.InductionEditExemptionReason(PersonId, JourneyInstance!.InstanceId),
            InductionJourneyPage.StartDate => _linkGenerator.InductionEditStartDate(PersonId, JourneyInstance!.InstanceId),
            InductionJourneyPage.ChangeReason => _linkGenerator.InductionChangeReason(PersonId, JourneyInstance!.InstanceId),
            InductionJourneyPage.CheckYourAnswers => _linkGenerator.InductionCheckYourAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => _linkGenerator.PersonInduction(PersonId)
        };
    }
}
