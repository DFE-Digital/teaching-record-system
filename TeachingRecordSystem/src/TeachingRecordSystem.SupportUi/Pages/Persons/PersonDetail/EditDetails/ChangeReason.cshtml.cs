using System.ComponentModel.DataAnnotations;
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
    public UploadEvidenceViewModel? UploadEvidence1 { get; set; }

    [BindProperty]
    public UploadEvidenceViewModel? UploadEvidence2 { get; set; }

    public string BackLink =>
        GetPageLink(FromCheckAnswers ? EditDetailsJourneyPage.CheckAnswers : EditDetailsJourneyPage.Index);

    public EditDetailsJourneyPage NextPage =>
        EditDetailsJourneyPage.CheckAnswers;

    public Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        UploadEvidence1 = JourneyInstance.State.UploadEvidence ?? new UploadEvidenceViewModel();
        UploadEvidence2 = JourneyInstance.State.UploadEvidence ?? new UploadEvidenceViewModel();

        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ChangeReason is not null && ChangeReason.Value == EditDetailsChangeReasonOption.AnotherReason && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter a reason");
        }

        await UploadEvidence2!.ValidateAsync(nameof(UploadEvidence2), ModelState, FileService);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail = ChangeReason is EditDetailsChangeReasonOption.AnotherReason ? ChangeReasonDetail : null;
            state.UploadEvidence = UploadEvidence1;
        });

        return Redirect(GetPageLink(EditDetailsJourneyPage.CheckAnswers));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);
    }
}
