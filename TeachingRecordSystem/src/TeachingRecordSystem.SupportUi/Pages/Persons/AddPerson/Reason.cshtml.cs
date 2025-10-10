using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), RequireJourneyInstance]
public class ReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you creating this record?")]
    public AddPersonReasonOption? Reason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public string BackLink => GetPageLink(
        FromCheckAnswers
            ? AddPersonJourneyPage.CheckAnswers
            : AddPersonJourneyPage.PersonalDetails);

    public string NextPage => GetPageLink(
        AddPersonJourneyPage.CheckAnswers,
        FromCheckAnswers is true ? true : null);

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < AddPersonJourneyPage.Reason)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }
    }

    public void OnGet()
    {
        Reason = JourneyInstance!.State.Reason;
        ReasonDetail = JourneyInstance.State.ReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Reason is not null && Reason.Value == AddPersonReasonOption.AnotherReason && ReasonDetail is null)
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
            state.Reason = Reason;
            state.ReasonDetail = Reason is AddPersonReasonOption.AnotherReason ? ReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(NextPage);
    }
}
