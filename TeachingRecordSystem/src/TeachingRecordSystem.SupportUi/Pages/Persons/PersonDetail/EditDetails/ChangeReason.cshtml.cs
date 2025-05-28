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
    public UploadEvidenceViewModel? UploadEvidence { get; set; }

    public string BackLink =>
        GetPageLink(FromCheckAnswers ? EditDetailsJourneyPage.CheckAnswers : EditDetailsJourneyPage.Index);

    public EditDetailsJourneyPage NextPage =>
        EditDetailsJourneyPage.CheckAnswers;

    public Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        UploadEvidence = JourneyInstance.State.UploadEvidence;

        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ChangeReason is not null && ChangeReason.Value == EditDetailsChangeReasonOption.AnotherReason && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter a reason");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail = ChangeReason is EditDetailsChangeReasonOption.AnotherReason ? ChangeReasonDetail : null;
            state.UploadEvidence = UploadEvidence;
        });

        return Redirect(GetPageLink(EditDetailsJourneyPage.CheckAnswers));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);
    }
}
