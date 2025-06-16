using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class NameChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing the name on this record?")]
    public EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence of this name change?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? NameChangeUploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? NameChangeEvidenceFile { get; set; }

    [BindProperty]
    public Guid? NameChangeEvidenceFileId { get; set; }

    [BindProperty]
    public string? NameChangeEvidenceFileName { get; set; }

    [BindProperty]
    public string? NameChangeEvidenceFileSizeDescription { get; set; }

    [BindProperty]
    public string? NameChangeUploadedEvidenceFileUrl { get; set; }

    public string BackLink => GetPageLink(
        FromCheckAnswers
            ? EditDetailsJourneyPage.CheckAnswers
            : EditDetailsJourneyPage.PersonalDetails);

    public string NextPage => GetPageLink(
        FromCheckAnswers
            ? EditDetailsJourneyPage.CheckAnswers
            : JourneyInstance!.State.OtherDetailsChanged
                ? EditDetailsJourneyPage.OtherDetailsChangeReason
                : EditDetailsJourneyPage.CheckAnswers,
        FromCheckAnswers is true ? true : null);

    public async Task OnGetAsync()
    {
        NameChangeReason = JourneyInstance!.State.NameChangeReason;
        NameChangeUploadEvidence = JourneyInstance.State.NameChangeUploadEvidence;
        NameChangeEvidenceFileId = JourneyInstance.State.NameChangeEvidenceFileId;
        NameChangeEvidenceFileName = JourneyInstance.State.NameChangeEvidenceFileName;
        NameChangeEvidenceFileSizeDescription = JourneyInstance.State.NameChangeEvidenceFileSizeDescription;
        NameChangeUploadedEvidenceFileUrl = JourneyInstance.State.NameChangeEvidenceFileId is null ? null :
            await FileService.GetFileUrlAsync(JourneyInstance.State.NameChangeEvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NameChangeUploadEvidence == true && NameChangeEvidenceFileId is null && NameChangeEvidenceFile is null)
        {
            ModelState.AddModelError(nameof(NameChangeEvidenceFile), "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (NameChangeEvidenceFileId.HasValue && (NameChangeEvidenceFile is not null || NameChangeUploadEvidence != true))
        {
            await FileService.DeleteFileAsync(NameChangeEvidenceFileId.Value);
            NameChangeEvidenceFileName = null;
            NameChangeEvidenceFileSizeDescription = null;
            NameChangeUploadedEvidenceFileUrl = null;
            NameChangeEvidenceFileId = null;
        }

        // Upload the file and set the display fields even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (NameChangeUploadEvidence == true && NameChangeEvidenceFile is not null)
        {
            using var stream = NameChangeEvidenceFile.OpenReadStream();
            var evidenceFileId = await FileService.UploadFileAsync(stream, NameChangeEvidenceFile.ContentType);
            NameChangeEvidenceFileId = evidenceFileId;
            NameChangeEvidenceFileName = NameChangeEvidenceFile?.FileName;
            NameChangeEvidenceFileSizeDescription = NameChangeEvidenceFile?.Length.Bytes().Humanize();
            NameChangeUploadedEvidenceFileUrl = await FileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.NameChangeReason = NameChangeReason;
            state.NameChangeUploadEvidence = NameChangeUploadEvidence;
            state.NameChangeEvidenceFileId = NameChangeUploadEvidence is true ? NameChangeEvidenceFileId : null;
            state.NameChangeEvidenceFileName = NameChangeUploadEvidence is true ? NameChangeEvidenceFileName : null;
            state.NameChangeEvidenceFileSizeDescription = NameChangeUploadEvidence is true ? NameChangeEvidenceFileSizeDescription : null;
        });

        return Redirect(NextPage);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < EditDetailsJourneyPage.NameChangeReason)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }
    }
}
