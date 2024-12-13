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

    //protected Func<Guid, JourneyInstanceId, string> PageLinkFunction(EditInductionState.InductionJourneyPage? pageName)
    //{
    //    return pageName switch
    //    {
    //        EditInductionState.InductionJourneyPage.Status => (Id, journeyInstanceId) => _linkGenerator.InductionEditStatus(Id, journeyInstanceId),
    //        EditInductionState.InductionJourneyPage.CompletedDate => (Id, journeyInstanceId) => _linkGenerator.InductionEditCompletedDate(Id, journeyInstanceId),
    //        EditInductionState.InductionJourneyPage.ExemptionReason => (Id, journeyInstanceId) => _linkGenerator.InductionEditExemptionReason(Id, journeyInstanceId),
    //        EditInductionState.InductionJourneyPage.StartDate => (Id, journeyInstanceId) => _linkGenerator.InductionEditStartDate(Id, journeyInstanceId),
    //        EditInductionState.InductionJourneyPage.ChangeReason => (Id, journeyInstanceId) => _linkGenerator.InductionChangeReason(Id, journeyInstanceId),
    //        EditInductionState.InductionJourneyPage.CheckYourAnswers => (Id, journeyInstanceId) => _linkGenerator.InductionCheckYourAnswers(Id, journeyInstanceId),
    //        _ => throw new NotImplementedException()
    //    };
    //}

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
