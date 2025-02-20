using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class InductionChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(linkGenerator)
{
    public string? PersonName { get; set; }

    [FromQuery]
    public JourneyFromCheckYourAnswersPage? FromCheckAnswers { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing the induction details?")]
    public InductionChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to add more information about why you’re changing the induction details?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you’re changing the induction details")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Add additional detail")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = "Additional detail must be 4000 characters or less")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = "The selected file must be smaller than 50MB")]
    public IFormFile? EvidenceFile { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;

    public InductionJourneyPage NextPage => InductionJourneyPage.CheckAnswers;

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresCompletedDate() => GetPageLink(InductionJourneyPage.CompletedDate),
                _ when InductionStatus.RequiresStartDate() => GetPageLink(InductionJourneyPage.StartDate),
                _ when InductionStatus.RequiresExemptionReasons() => GetPageLink(InductionJourneyPage.ExemptionReason),
                _ => GetPageLink(InductionJourneyPage.Status),
            };
        }
    }

    public async Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance!.State.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance?.State.ChangeReasonDetail;
        UploadEvidence = JourneyInstance?.State.UploadEvidence;
        UploadedEvidenceFileUrl = JourneyInstance?.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
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
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail = ChangeReasonDetail;
            state.UploadEvidence = UploadEvidence;
        });

        return Redirect(GetPageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(dbContext, PersonId, InductionJourneyPage.Status);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        EvidenceFileId = JourneyInstance!.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;

        await next();
    }
}
