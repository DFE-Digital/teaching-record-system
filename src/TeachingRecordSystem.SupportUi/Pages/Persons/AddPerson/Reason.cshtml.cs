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
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ReasonDetail)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.ReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.Reason == PersonCreateReason.AnotherReason),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    [BindProperty]
    public PersonCreateReason? Reason { get; set; }

    [BindProperty]
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
        await EvidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);
        _validator.ValidateAndThrow(this);

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
