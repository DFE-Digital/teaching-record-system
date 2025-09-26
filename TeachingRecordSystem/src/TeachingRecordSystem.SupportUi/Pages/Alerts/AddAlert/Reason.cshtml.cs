using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class ReasonModel(TrsLinkGenerator linkGenerator, IFileService fileService) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Select a reason")]
    [Required(ErrorMessage = "Select a reason")]
    public AddAlertReasonOption? AddReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to add more information about why you’re adding this alert?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you’re adding this alert")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Add additional detail")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? AddReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task OnGetAsync()
    {
        AddReason = JourneyInstance!.State.AddReason;
        HasAdditionalReasonDetail = JourneyInstance!.State.HasAdditionalReasonDetail;
        AddReasonDetail = JourneyInstance!.State.AddReasonDetail;
        UploadedEvidenceFileUrl = JourneyInstance?.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
        UploadEvidence = JourneyInstance?.State.UploadEvidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && AddReasonDetail is null)
        {
            ModelState.AddModelError(nameof(AddReasonDetail), "Enter additional detail");
        }

        if (UploadEvidence == true && EvidenceFileId is null && EvidenceFile is null)
        {
            ModelState.AddModelError(nameof(EvidenceFile), "Select a file");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        if (UploadEvidence == true)
        {
            if (EvidenceFile is not null)
            {
                if (EvidenceFileId is not null)
                {
                    await fileService.DeleteFileAsync(EvidenceFileId.Value);
                }

                using var stream = EvidenceFile.OpenReadStream();
                var evidenceFileId = await fileService.UploadFileAsync(stream, EvidenceFile.ContentType);
                await JourneyInstance!.UpdateStateAsync(state =>
                {
                    state.EvidenceFileId = evidenceFileId;
                    state.EvidenceFileName = EvidenceFile.FileName;
                    state.EvidenceFileSizeDescription = EvidenceFile.Length.Bytes().Humanize();
                });
            }
        }
        else if (EvidenceFileId is not null)
        {
            await fileService.DeleteFileAsync(EvidenceFileId.Value);
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.EvidenceFileId = null;
                state.EvidenceFileName = null;
                state.EvidenceFileSizeDescription = null;
            });
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.AddReason = AddReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.AddReasonDetail = AddReasonDetail;
            state.UploadEvidence = UploadEvidence;
        });

        return Redirect(linkGenerator.AlertAddCheckAnswers(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.StartDate is null)
        {
            context.Result = Redirect(linkGenerator.AlertAddStartDate(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        EvidenceFileId = JourneyInstance!.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
    }
}
