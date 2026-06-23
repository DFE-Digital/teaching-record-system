using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class ReasonModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.ChangeReason == PersonInductionChangeReason.AnotherReason),
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information about why you’re changing the induction details"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter details")
            .When(m => m.ProvideAdditionalInformation == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    [BindProperty]
    public PersonInductionChangeReason? ChangeReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;

    public InductionJourneyPage NextPage => InductionJourneyPage.CheckAnswers;

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckAnswersPage.CheckAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresCompletedDate() => GetPageLink(InductionJourneyPage.CompletedDate),
                _ when InductionStatus.RequiresStartDate() => GetPageLink(InductionJourneyPage.StartDate),
                _ when InductionStatus.RequiresExemptionReasons() => GetPageLink(InductionJourneyPage.ExemptionReason),
                _ => GetPageLink(InductionJourneyPage.Status)
            };
        }
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ProvideAdditionalInformation = JourneyInstance.State.ProvideAdditionalInformation;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReason == PersonInductionChangeReason.AnotherReason ? JourneyInstance.State.ChangeReasonDetail : null;
        AdditionalInformation = JourneyInstance.State.ProvideAdditionalInformation == true ? JourneyInstance.State.AdditionalInformation : null;
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
            state.ChangeReason = ChangeReason;
            state.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.AdditionalInformation = ProvideAdditionalInformation is true ? AdditionalInformation : null;
            state.ChangeReasonDetail = ChangeReason == PersonInductionChangeReason.AnotherReason ? ChangeReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(GetPageLink(NextPage));
    }
}
