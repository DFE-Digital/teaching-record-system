using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public abstract class CommonJourneyPage : PageModel
{
    protected TrsLinkGenerator LinkGenerator { get; set; }
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    protected CommonJourneyPage(TrsLinkGenerator linkGenerator)
    {
        LinkGenerator = linkGenerator;
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonInduction(PersonId));
    }

    protected string PageLink(InductionJourneyPage? pageName, JourneyFromCheckYourAnswersPage? fromCheckYourAnswersPage = null)
    {
        return pageName switch
        {
            InductionJourneyPage.Status => LinkGenerator.InductionEditStatus(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.CompletedDate => LinkGenerator.InductionEditCompletedDate(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.ExemptionReason => LinkGenerator.InductionEditExemptionReason(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.StartDate => LinkGenerator.InductionEditStartDate(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.ChangeReasons => LinkGenerator.InductionChangeReason(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.CheckAnswers => LinkGenerator.InductionCheckYourAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonInduction(PersonId)
        };
    }
}
