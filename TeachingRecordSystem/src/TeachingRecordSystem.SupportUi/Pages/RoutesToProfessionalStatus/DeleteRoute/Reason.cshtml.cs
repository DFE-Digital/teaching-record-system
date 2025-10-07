using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class ReasonModel(TrsLinkGenerator linkGenerator,
    IFileService fileService) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<DeleteRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you deleting this route?")]
    public ChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to provide more information?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you\u2019re deleting this route")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Enter details about this change")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

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

    public string NextPage => linkGenerator.RouteDeleteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId);

    public string BackLink => FromCheckAnswers == true
        ? linkGenerator.RouteDeleteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId)
        : linkGenerator.PersonQualifications(PersonId);

    public async Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance!.State.ChangeReasonDetail.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance?.State.ChangeReasonDetail.ChangeReasonDetail;
        UploadEvidence = JourneyInstance?.State.ChangeReasonDetail.UploadEvidence;
        UploadedEvidenceFileUrl = JourneyInstance?.State.ChangeReasonDetail.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance.State.ChangeReasonDetail.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter additional detail");
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
                    state.ChangeReasonDetail.EvidenceFileId = evidenceFileId;
                    state.ChangeReasonDetail.EvidenceFileName = EvidenceFile.FileName;
                    state.ChangeReasonDetail.EvidenceFileSizeDescription = EvidenceFile.Length.Bytes().Humanize();
                });
            }
        }
        else if (EvidenceFileId is not null)
        {
            await fileService.DeleteFileAsync(EvidenceFileId.Value);
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.ChangeReasonDetail.EvidenceFileId = null;
                state.ChangeReasonDetail.EvidenceFileName = null;
                state.ChangeReasonDetail.EvidenceFileSizeDescription = null;
            });
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail;
            state.ChangeReasonDetail.UploadEvidence = UploadEvidence;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
        });

        return Redirect(NextPage);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        EvidenceFileId = JourneyInstance!.State.ChangeReasonDetail.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.ChangeReasonDetail.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.ChangeReasonDetail.EvidenceFileSizeDescription;

        return next();
    }
}
