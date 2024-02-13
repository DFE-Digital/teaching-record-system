using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

[Journey(JourneyNames.EditMqProvider), RequireJourneyInstance]
public class ReasonModel(TrsLinkGenerator linkGenerator, IFileService fileService) : PageModel
{
    public const int MaxFileSizeMb = 50;

    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<EditMqProviderState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Reason for change")]
    [Required(ErrorMessage = "Select a reason for change")]
    public MqChangeProviderReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "More detail about the reason for change")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Upload evidence")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(MaxFileSizeMb * 1024 * 1024, ErrorMessage = "The selected file must be smaller than 50MB")]
    public IFormFile? EvidenceFile { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task OnGet()
    {
        ChangeReason ??= JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail ??= JourneyInstance?.State.ChangeReasonDetail;
        UploadedEvidenceFileUrl ??= JourneyInstance?.State.EvidenceFileId is not null ?
            await fileService.GetFileUrl(JourneyInstance.State.EvidenceFileId.Value, _fileUrlExpiresAfter) :
            null;
        UploadEvidence ??= JourneyInstance?.State.UploadEvidence;
    }

    public async Task<IActionResult> OnPost()
    {
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
                    await fileService.DeleteFile(EvidenceFileId.Value);
                }

                using var stream = EvidenceFile.OpenReadStream();
                var evidenceFileId = await fileService.UploadFile(stream, EvidenceFile.ContentType);
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
            await fileService.DeleteFile(EvidenceFileId.Value);
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.EvidenceFileId = null;
                state.EvidenceFileName = null;
                state.EvidenceFileSizeDescription = null;
            });
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail = ChangeReasonDetail;
            state.UploadEvidence = UploadEvidence;
        });

        return Redirect(linkGenerator.MqEditProviderConfirm(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        if (JourneyInstance!.State.EvidenceFileId is not null)
        {
            await fileService.DeleteFile(JourneyInstance!.State.EvidenceFileId.Value);
        }

        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (string.IsNullOrEmpty(JourneyInstance!.State.MqEstablishmentValue))
        {
            context.Result = Redirect(linkGenerator.MqEditProvider(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        EvidenceFileId = JourneyInstance!.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
    }
}
