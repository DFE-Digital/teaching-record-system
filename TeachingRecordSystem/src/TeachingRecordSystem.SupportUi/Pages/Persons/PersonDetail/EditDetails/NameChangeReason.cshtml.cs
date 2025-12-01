using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class NameChangeReasonModel(
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(personService, linkGenerator, evidenceUploadManager)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    public EditDetailsNameChangeReasonOption? Reason { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public string BackLink => GetPageLink(
        FromCheckAnswers
            ? EditDetailsJourneyPage.CheckAnswers
            : EditDetailsJourneyPage.PersonalDetails);

    public string NextPage => GetPageLink(
        FromCheckAnswers
            ? EditDetailsJourneyPage.CheckAnswers
            : JourneyInstance!.State.OtherDetailsChanged
                ? EditDetailsJourneyPage.OtherDetailsChangeReason
                : EditDetailsJourneyPage.CheckAnswers,
        FromCheckAnswers is true ? true : null);

    public void OnGet()
    {
        Reason = JourneyInstance!.State.NameChangeReason;
        Evidence = JourneyInstance.State.NameChangeEvidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await EvidenceUploadManager.ValidateAndUploadAsync<NameChangeReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.NameChangeReason = Reason;
            state.NameChangeEvidence = Evidence;
        });

        return Redirect(NextPage);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < EditDetailsJourneyPage.NameChangeReason)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }
    }
}
