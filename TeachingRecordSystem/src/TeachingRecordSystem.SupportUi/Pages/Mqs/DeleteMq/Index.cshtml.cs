using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[Journey(JourneyNames.DeleteMq), ActivatesJourney, RequireJourneyInstance]
public class IndexModel : PageModel
{
    public const int MaxFileSizeMb = 50;
    private readonly TimeSpan FileUrlExpiresAfter = TimeSpan.FromMinutes(15);
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly IFileService _fileService;

    public IndexModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache,
        TrsLinkGenerator linkGenerator,
        IFileService fileService)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _referenceDataCache = referenceDataCache;
        _linkGenerator = linkGenerator;
        _fileService = fileService;
    }

    public JourneyInstance<DeleteMqState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? TrainingProvider { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [BindProperty]
    [Display(Name = "Reason for deleting")]
    public MqDeletionReasonOption? DeletionReason { get; set; }

    [BindProperty]
    [Display(Name = "More detail about the reason for deleting")]
    public string? DeletionReasonDetail { get; set; }

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
        DeletionReason ??= JourneyInstance!.State.DeletionReason;
        DeletionReasonDetail ??= JourneyInstance?.State.DeletionReasonDetail;
        UploadedEvidenceFileUrl ??= JourneyInstance?.State.EvidenceFileId is not null ? await _fileService.GetFileUrl(JourneyInstance.State.EvidenceFileId.Value, FileUrlExpiresAfter) : null;
        UploadEvidence ??= JourneyInstance?.State.UploadEvidence;
    }

    public async Task<IActionResult> OnPost()
    {
        if (DeletionReason is null)
        {
            ModelState.AddModelError(nameof(DeletionReason), "Select a reason for deleting");
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
                    await _fileService.DeleteFile(EvidenceFileId.Value);
                }

                using var stream = EvidenceFile.OpenReadStream();
                var evidenceFileId = await _fileService.UploadFile(stream, EvidenceFile.ContentType);
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
            await _fileService.DeleteFile(EvidenceFileId.Value);
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.EvidenceFileId = null;
                state.EvidenceFileName = null;
                state.EvidenceFileSizeDescription = null;
            });
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.DeletionReason = DeletionReason;
            state.DeletionReasonDetail = DeletionReasonDetail;
            state.UploadEvidence = UploadEvidence;
        });

        return Redirect(_linkGenerator.MqDeleteConfirm(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        if (JourneyInstance!.State.EvidenceFileId is not null)
        {
            await _fileService.DeleteFile(JourneyInstance!.State.EvidenceFileId.Value);
        }

        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var qualification = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId));
        if (qualification is null || qualification.dfeta_Type != dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            context.Result = NotFound();
            return;
        }

        await JourneyInstance!.State.EnsureInitialized(_crmQueryDispatcher, _referenceDataCache, qualification);

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;
        TrainingProvider = JourneyInstance!.State.MqEstablishment;
        Specialism = JourneyInstance!.State.Specialism;
        Status = JourneyInstance!.State.Status;
        StartDate = JourneyInstance!.State.StartDate;
        EndDate = JourneyInstance!.State.EndDate;
        EvidenceFileId = JourneyInstance!.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;

        await next();
    }
}
