using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), RequireJourneyInstance]
public class ReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you creating this record?")]
    public AddPersonReasonOption? CreateReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? CreateReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    [BindProperty]
    public Guid? EvidenceFileId { get; set; }

    [BindProperty]
    public string? EvidenceFileName { get; set; }

    [BindProperty]
    public string? EvidenceFileSizeDescription { get; set; }

    [BindProperty]
    public string? EvidenceFileUrl { get; set; }

    public string BackLink => GetPageLink(
        FromCheckAnswers
            ? AddPersonJourneyPage.CheckAnswers
            : AddPersonJourneyPage.PersonalDetails);

    public string NextPage => GetPageLink(
        AddPersonJourneyPage.CheckAnswers,
        FromCheckAnswers is true ? true : null);

    public async Task OnGetAsync()
    {
        CreateReason = JourneyInstance!.State.CreateReason;
        CreateReasonDetail = JourneyInstance.State.CreateReasonDetail;
        UploadEvidence = JourneyInstance.State.UploadEvidence;
        EvidenceFileId = JourneyInstance.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance.State.EvidenceFileSizeDescription;
        EvidenceFileUrl = JourneyInstance.State.EvidenceFileId is null ? null :
            await FileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CreateReason is not null && CreateReason.Value == AddPersonReasonOption.AnotherReason && CreateReasonDetail is null)
        {
            ModelState.AddModelError(nameof(CreateReasonDetail), "Enter a reason");
        }

        if (UploadEvidence == true && EvidenceFileId is null && EvidenceFile is null)
        {
            ModelState.AddModelError(nameof(EvidenceFile), "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (EvidenceFileId.HasValue && (EvidenceFile is not null || UploadEvidence != true))
        {
            await FileService.DeleteFileAsync(EvidenceFileId.Value);
            EvidenceFileName = null;
            EvidenceFileSizeDescription = null;
            EvidenceFileUrl = null;
            EvidenceFileId = null;
        }

        // Upload the file and set the display fields even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (UploadEvidence == true && EvidenceFile is not null)
        {
            using var stream = EvidenceFile.OpenReadStream();
            var evidenceFileId = await FileService.UploadFileAsync(stream, EvidenceFile.ContentType);
            EvidenceFileId = evidenceFileId;
            EvidenceFileName = EvidenceFile?.FileName;
            EvidenceFileSizeDescription = EvidenceFile?.Length.Bytes().Humanize();
            EvidenceFileUrl = await FileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.CreateReason = CreateReason;
            state.CreateReasonDetail = CreateReason is AddPersonReasonOption.AnotherReason ? CreateReasonDetail : null;
            state.UploadEvidence = UploadEvidence;
            state.EvidenceFileId = UploadEvidence is true ? EvidenceFileId : null;
            state.EvidenceFileName = UploadEvidence is true ? EvidenceFileName : null;
            state.EvidenceFileSizeDescription = UploadEvidence is true ? EvidenceFileSizeDescription : null;
        });

        return Redirect(NextPage);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < AddPersonJourneyPage.Reason)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }
    }
}
