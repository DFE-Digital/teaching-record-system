using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Views.Shared.Components.UploadEvidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class ChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
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
    public UploadEvidenceViewModel? UploadEvidence { get; set; }

    public string BackLink =>
        GetPageLink(FromCheckAnswers ? EditDetailsJourneyPage.CheckAnswers : EditDetailsJourneyPage.Index);

    public EditDetailsJourneyPage NextPage =>
        EditDetailsJourneyPage.CheckAnswers;

    public async Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        UploadEvidence = new UploadEvidenceViewModel
        {
            UploadEvidence = JourneyInstance.State.UploadEvidence,
            EvidenceFileId = JourneyInstance.State.EvidenceFileId,
            EvidenceFileName = JourneyInstance.State.EvidenceFileName,
            EvidenceFileSizeDescription = JourneyInstance.State.EvidenceFileSizeDescription,
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ChangeReason is not null && ChangeReason.Value == EditDetailsChangeReasonOption.AnotherReason && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter a reason");
        }

        if (UploadEvidence!.UploadEvidence == true && UploadEvidence!.EvidenceFileId is null && UploadEvidence!.EvidenceFile is null)
        {
            ModelState.AddModelError("EvidenceFile", "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (UploadEvidence!.EvidenceFileId.HasValue && (UploadEvidence!.EvidenceFile is not null || UploadEvidence!.UploadEvidence != true))
        {
            await FileService.DeleteFileAsync(UploadEvidence!.EvidenceFileId.Value);
            UploadEvidence!.EvidenceFileName = null;
            UploadEvidence!.EvidenceFileSizeDescription = null;
            UploadEvidence!.UploadedEvidenceFileUrl = null;
            UploadEvidence!.EvidenceFileId = null;
        }

        // Upload the file even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (UploadEvidence!.UploadEvidence == true)
        {
            // Upload the file and set the display fields
            if (UploadEvidence!.EvidenceFile is not null)
            {
                using var stream = UploadEvidence!.EvidenceFile.OpenReadStream();
                var evidenceFileId = await FileService.UploadFileAsync(stream, UploadEvidence!.EvidenceFile.ContentType);
                UploadEvidence!.EvidenceFileId = evidenceFileId;
                UploadEvidence!.EvidenceFileName = UploadEvidence!.EvidenceFile?.FileName;
                UploadEvidence!.EvidenceFileSizeDescription = UploadEvidence!.EvidenceFile?.Length.Bytes().Humanize();
                UploadEvidence!.UploadedEvidenceFileUrl = await FileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail = ChangeReason is EditDetailsChangeReasonOption.AnotherReason ? ChangeReasonDetail : null;
            state.UploadEvidence = UploadEvidence!.UploadEvidence;
            state.EvidenceFileId = UploadEvidence!.UploadEvidence is true ? UploadEvidence!.EvidenceFileId : null;
            state.EvidenceFileName = UploadEvidence!.UploadEvidence is true ? UploadEvidence!.EvidenceFileName : null;
            state.EvidenceFileSizeDescription = UploadEvidence!.UploadEvidence is true ? UploadEvidence!.EvidenceFileSizeDescription : null;
        });

        return Redirect(GetPageLink(EditDetailsJourneyPage.CheckAnswers));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);
    }
}
