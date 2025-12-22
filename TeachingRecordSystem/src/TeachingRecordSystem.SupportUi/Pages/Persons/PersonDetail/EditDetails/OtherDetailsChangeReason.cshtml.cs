using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class OtherDetailsChangeReasonModel(
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(personService, linkGenerator, evidenceUploadManager)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    public PersonDetailsChangeReason? Reason { get; set; }

    [BindProperty]
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
        if (Reason is PersonDetailsChangeReason.AnotherReason && ReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ReasonDetail), "Enter a reason");
        }

        await EvidenceUploadManager.ValidateAndUploadAsync<OtherDetailsChangeReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.OtherDetailsChangeReason = Reason;
            state.OtherDetailsChangeReasonDetail = Reason is PersonDetailsChangeReason.AnotherReason ? ReasonDetail : null;
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
