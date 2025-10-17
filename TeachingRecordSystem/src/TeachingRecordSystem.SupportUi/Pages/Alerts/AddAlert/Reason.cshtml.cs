using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    public AddAlertReasonOption? AddReason { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you’re adding this alert")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? AddReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.StartDate is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.AddAlert.StartDate(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        AddReason = JourneyInstance!.State.AddReason;
        HasAdditionalReasonDetail = JourneyInstance.State.HasAdditionalReasonDetail;
        AddReasonDetail = JourneyInstance.State.AddReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && AddReasonDetail is null)
        {
            ModelState.AddModelError(nameof(AddReasonDetail), "Enter additional detail");
        }

        await evidenceUploadManager.ValidateAndUploadAsync(Evidence, ModelState);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.AddReason = AddReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.AddReasonDetail = AddReasonDetail;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Alerts.AddAlert.CheckAnswers(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
