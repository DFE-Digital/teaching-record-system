using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class NameChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceController evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing the name on this record?")]
    public EditDetailsNameChangeReasonOption? Reason { get; set; }

    [BindProperty]
    public EvidenceModel Evidence { get; set; } = new();

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
        await EvidenceController.ValidateAndUploadAsync(Evidence, ModelState);

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
