using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class ChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing this record?")]
    public EditDetailsChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
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

    public string BackLink =>
        GetPageLink(FromCheckAnswers ? EditDetailsJourneyPage.CheckAnswers : EditDetailsJourneyPage.Index);

    public EditDetailsJourneyPage NextPage =>
        EditDetailsJourneyPage.CheckAnswers;

    public async Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance?.State.ChangeReasonDetail;
        UploadEvidence = JourneyInstance?.State.UploadEvidence;
        UploadedEvidenceFileUrl = JourneyInstance?.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ChangeReason is not null && ChangeReason.Value == EditDetailsChangeReasonOption.AnotherReason && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter a reason");
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
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail = ChangeReasonDetail;
            state.UploadEvidence = UploadEvidence;
        });

        return Redirect(GetPageLink(EditDetailsJourneyPage.CheckAnswers));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        EvidenceFileId = JourneyInstance!.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
    }
}
