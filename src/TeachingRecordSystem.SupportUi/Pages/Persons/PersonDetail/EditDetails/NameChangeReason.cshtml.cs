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
    private readonly InlineValidator<NameChangeReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.Evidence).Evidence(),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.ReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.Reason == PersonNameChangeReason.AnotherReason),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotNull().WithMessage("Enter details")
            .MaximumLength(4000).WithMessage("Additional detail must be 4000 characters or less")
            .When(x => x.ProvideAdditionalInformation == true)
    };

    [BindProperty]
    public string? ReasonDetail { get; set; }

    [BindProperty]
    public PersonNameChangeReason? Reason { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

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
        ReasonDetail = JourneyInstance.State.NameChangeReasonDetail;
        Evidence = JourneyInstance.State.NameChangeEvidence;
        ProvideAdditionalInformation = JourneyInstance.State.ProvideAdditionalInformation;
        AdditionalInformation = JourneyInstance.State.AdditionalInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await EvidenceUploadManager.ValidateAndUploadAsync<NameChangeReasonModel>(m => m.Evidence, ViewData);
        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.NameChangeReason = Reason;
            state.NameChangeReasonDetail = Reason is PersonNameChangeReason.AnotherReason ? ReasonDetail : null;
            state.NameChangeEvidence = Evidence;
            state.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.AdditionalInformation = ProvideAdditionalInformation == true ? AdditionalInformation : null;
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
