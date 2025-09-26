using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class OtherDetailsChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing this record?")]
    public EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? OtherDetailsChangeReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? OtherDetailsChangeUploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? OtherDetailsChangeEvidenceFile { get; set; }

    [BindProperty]
    public Guid? OtherDetailsChangeEvidenceFileId { get; set; }

    [BindProperty]
    public string? OtherDetailsChangeEvidenceFileName { get; set; }

    [BindProperty]
    public string? OtherDetailsChangeEvidenceFileSizeDescription { get; set; }

    [BindProperty]
    public string? OtherDetailsChangeUploadedEvidenceFileUrl { get; set; }

    public bool IsAlsoChangingName => JourneyInstance!.State.NameChangeReason is not null;

    public string BackLink => GetPageLink(
        FromCheckAnswers
            ? EditDetailsJourneyPage.CheckAnswers
            : JourneyInstance!.State.NameChanged
                ? EditDetailsJourneyPage.NameChangeReason
                : EditDetailsJourneyPage.PersonalDetails);

    public string NextPage => GetPageLink(
        EditDetailsJourneyPage.CheckAnswers,
        FromCheckAnswers is true ? true : null);

    public async Task OnGetAsync()
    {
        OtherDetailsChangeReason = JourneyInstance!.State.OtherDetailsChangeReason;
        OtherDetailsChangeReasonDetail = JourneyInstance.State.OtherDetailsChangeReasonDetail;
        OtherDetailsChangeUploadEvidence = JourneyInstance.State.OtherDetailsChangeUploadEvidence;
        OtherDetailsChangeEvidenceFileId = JourneyInstance.State.OtherDetailsChangeEvidenceFileId;
        OtherDetailsChangeEvidenceFileName = JourneyInstance.State.OtherDetailsChangeEvidenceFileName;
        OtherDetailsChangeEvidenceFileSizeDescription = JourneyInstance.State.OtherDetailsChangeEvidenceFileSizeDescription;
        OtherDetailsChangeUploadedEvidenceFileUrl = JourneyInstance.State.OtherDetailsChangeEvidenceFileId is null ? null :
            await FileService.GetFileUrlAsync(JourneyInstance.State.OtherDetailsChangeEvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (OtherDetailsChangeReason is not null && OtherDetailsChangeReason.Value == EditDetailsOtherDetailsChangeReasonOption.AnotherReason && OtherDetailsChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(OtherDetailsChangeReasonDetail), "Enter a reason");
        }

        if (OtherDetailsChangeUploadEvidence == true && OtherDetailsChangeEvidenceFileId is null && OtherDetailsChangeEvidenceFile is null)
        {
            ModelState.AddModelError(nameof(OtherDetailsChangeEvidenceFile), "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (OtherDetailsChangeEvidenceFileId.HasValue && (OtherDetailsChangeEvidenceFile is not null || OtherDetailsChangeUploadEvidence != true))
        {
            await FileService.DeleteFileAsync(OtherDetailsChangeEvidenceFileId.Value);
            OtherDetailsChangeEvidenceFileName = null;
            OtherDetailsChangeEvidenceFileSizeDescription = null;
            OtherDetailsChangeUploadedEvidenceFileUrl = null;
            OtherDetailsChangeEvidenceFileId = null;
        }

        // Upload the file and set the display fields even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (OtherDetailsChangeUploadEvidence == true && OtherDetailsChangeEvidenceFile is not null)
        {
            using var stream = OtherDetailsChangeEvidenceFile.OpenReadStream();
            var evidenceFileId = await FileService.UploadFileAsync(stream, OtherDetailsChangeEvidenceFile.ContentType);
            OtherDetailsChangeEvidenceFileId = evidenceFileId;
            OtherDetailsChangeEvidenceFileName = OtherDetailsChangeEvidenceFile?.FileName;
            OtherDetailsChangeEvidenceFileSizeDescription = OtherDetailsChangeEvidenceFile?.Length.Bytes().Humanize();
            OtherDetailsChangeUploadedEvidenceFileUrl = await FileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.OtherDetailsChangeReason = OtherDetailsChangeReason;
            state.OtherDetailsChangeReasonDetail = OtherDetailsChangeReason is EditDetailsOtherDetailsChangeReasonOption.AnotherReason ? OtherDetailsChangeReasonDetail : null;
            state.OtherDetailsChangeUploadEvidence = OtherDetailsChangeUploadEvidence;
            state.OtherDetailsChangeEvidenceFileId = OtherDetailsChangeUploadEvidence is true ? OtherDetailsChangeEvidenceFileId : null;
            state.OtherDetailsChangeEvidenceFileName = OtherDetailsChangeUploadEvidence is true ? OtherDetailsChangeEvidenceFileName : null;
            state.OtherDetailsChangeEvidenceFileSizeDescription = OtherDetailsChangeUploadEvidence is true ? OtherDetailsChangeEvidenceFileSizeDescription : null;
        });

        return Redirect(NextPage);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < EditDetailsJourneyPage.OtherDetailsChangeReason)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }
    }
}
