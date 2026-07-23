using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public abstract class CommonJourneyPage(
    AddPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    protected AddPersonJourneyCoordinator Journey { get; } = journey;
    protected SupportUiLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected EvidenceUploadManager EvidenceUploadManager { get; } = evidenceUploadManager;

    public JourneyInstanceId InstanceId => Journey.InstanceId;

    [BindProperty]
    public bool Cancel { get; set; }

    protected async Task<IActionResult> CancelAsync()
    {
        await EvidenceUploadManager.DeleteUploadedFileAsync(Journey.State.Evidence.UploadedEvidenceFile);
        Journey.DeleteInstance();
        return Redirect(LinkGenerator.Persons.AddPerson.Index());
    }

    protected string GetPageLink(AddPersonJourneyPage? pageName, string? returnUrl = null)
    {
        return pageName switch
        {
            AddPersonJourneyPage.PersonalDetails => LinkGenerator.Persons.AddPerson.PersonalDetails(Journey.InstanceId, returnUrl),
            AddPersonJourneyPage.Reason => LinkGenerator.Persons.AddPerson.Reason(Journey.InstanceId, returnUrl),
            AddPersonJourneyPage.CheckAnswers => LinkGenerator.Persons.AddPerson.CheckAnswers(Journey.InstanceId),
            _ => LinkGenerator.Persons.AddPerson.Index()
        };
    }
}
