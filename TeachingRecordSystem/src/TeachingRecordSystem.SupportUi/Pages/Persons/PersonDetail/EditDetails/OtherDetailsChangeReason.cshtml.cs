using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class OtherDetailsChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing this record?")]
    public EditDetailsOtherDetailsChangeReasonOption? Reason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

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

    public void OnGet()
    {
        Reason = JourneyInstance!.State.OtherDetailsChangeReason;
        ReasonDetail = JourneyInstance.State.OtherDetailsChangeReasonDetail;
        Evidence = JourneyInstance.State.OtherDetailsChangeEvidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Reason is EditDetailsOtherDetailsChangeReasonOption.AnotherReason && ReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ReasonDetail), "Enter a reason");
        }

        await EvidenceController.ValidateAndUploadAsync(Evidence, ModelState);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.OtherDetailsChangeReason = Reason;
            state.OtherDetailsChangeReasonDetail = Reason is EditDetailsOtherDetailsChangeReasonOption.AnotherReason ? ReasonDetail : null;
            state.OtherDetailsChangeEvidence = Evidence;
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
