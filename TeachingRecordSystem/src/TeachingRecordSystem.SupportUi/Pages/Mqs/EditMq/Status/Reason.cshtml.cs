using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[Journey(JourneyNames.EditMqStatus), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<EditMqStatusState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public MqChangeStatusReasonOption? StatusChangeReason { get; set; }

    [BindProperty]
    public MqChangeEndDateReasonOption? EndDateChangeReason { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to add more information")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool? IsEndDateChange { get; set; }

    public bool? IsStatusChange { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.Status is null
            || JourneyInstance.State.Status == MandatoryQualificationStatus.Passed && !JourneyInstance.State.EndDate.HasValue)
        {
            context.Result = Redirect(linkGenerator.Mqs.EditMq.Status.Index(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        IsEndDateChange = JourneyInstance?.State.IsEndDateChange;
        IsStatusChange = JourneyInstance?.State.IsStatusChange;
    }

    public void OnGet()
    {
        StatusChangeReason = JourneyInstance!.State.StatusChangeReason;
        EndDateChangeReason = JourneyInstance.State.EndDateChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (IsEndDateChange == true && IsStatusChange == false && EndDateChangeReason is null)
        {
            ModelState.AddModelError(nameof(EndDateChangeReason), "Select a reason");
        }

        if (IsStatusChange == true && StatusChangeReason is null)
        {
            ModelState.AddModelError(nameof(StatusChangeReason), "Select a reason");
        }

        await evidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.StatusChangeReason = StatusChangeReason;
            state.EndDateChangeReason = EndDateChangeReason;
            state.ChangeReasonDetail = ChangeReasonDetail;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Mqs.EditMq.Status.CheckAnswers(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
