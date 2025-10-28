using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

[Journey(JourneyNames.EditAlertLink), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public const int MaxFileSizeMb = 50;
    public const int ChangeReasonDetailMaxLength = 4000;

    public JourneyInstance<EditAlertLinkState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    public AlertChangeLinkReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to add more information")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [MaxLength(ChangeReasonDetailMaxLength, ErrorMessage = "Additional detail must be 4000 characters or less")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.AddLink is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.EditAlert.Link.Index(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance.State.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && string.IsNullOrEmpty(ChangeReasonDetail))
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter additional detail");
        }

        await evidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail = HasAdditionalReasonDetail!.Value ? ChangeReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Alerts.EditAlert.Link.CheckAnswers(AlertId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
