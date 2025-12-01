using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), RequireJourneyInstance]
public class ReasonModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(linkGenerator, evidenceUploadManager)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    public PersonCreateReason? Reason { get; set; }

    [BindProperty]
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
        if (Reason is not null && Reason.Value == PersonCreateReason.AnotherReason && ReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ReasonDetail), "Enter a reason");
        }

        await EvidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Reason = Reason;
            state.ReasonDetail = Reason is PersonCreateReason.AnotherReason ? ReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(NextPage);
    }
}
