using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), RequireJourneyInstance]
[AllowDeactivatedPerson]
public class ReasonModel(
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(personService, linkGenerator, evidenceController)
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        //de-activate rules
        v => v.RuleFor(m => m.DeactivateReason)
            .NotEmpty()
            .WithMessage("Select a reason")
            .When(x=>x.TargetStatus == PersonStatus.Deactivated),

        v => v.RuleFor(m => m.ProvideMoreInformation)
            .NotNull()
            .WithMessage("Select yes if you want to provide more information")
            .When(x=>x.TargetStatus == PersonStatus.Deactivated),

        v => v.RuleFor(m => m.DeactivateReasonDetail)
            .NotEmpty()
            .WithMessage("Enter a reason")
            .When(x => x.ProvideMoreInformation == ProvideMoreInformationOption.Yes && x.TargetStatus == PersonStatus.Deactivated),

        //re-activate rules
        v => v.RuleFor(m => m.ReactivateReason)
            .NotEmpty()
            .WithMessage("Select a reason")
            .When(x=>x.TargetStatus == PersonStatus.Active),

        v => v.RuleFor(m => m.ProvideMoreInformation)
            .NotNull()
            .WithMessage("Select yes if you want to provide more information")
            .When(x=>x.TargetStatus == PersonStatus.Active),

        v => v.RuleFor(m => m.ReactivateReasonDetail)
            .NotEmpty()
            .WithMessage("Enter a reason")
            .When(x => x.ProvideMoreInformation == ProvideMoreInformationOption.Yes && x.TargetStatus == PersonStatus.Active),

        // Make sure to take into account evidence models validation rules.
        v => v.RuleFor(x => x.Evidence).Evidence()
    };

    [BindProperty]
    public PersonDeactivateReason? DeactivateReason { get; set; }

    [BindProperty]
    public ProvideMoreInformationOption? ProvideMoreInformation { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.ReasonDetailsMaxCharacterCount, ErrorMessage = $"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}")]
    public string? DeactivateReasonDetail { get; set; }

    [BindProperty]
    public PersonReactivateReason? ReactivateReason { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.ReasonDetailsMaxCharacterCount, ErrorMessage = $"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}")]
    public string? ReactivateReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public string BackLink => FromCheckAnswers
        ? LinkGenerator.Persons.PersonDetail.SetStatus.CheckAnswers(PersonId, TargetStatus, JourneyInstance!.InstanceId)
        : LinkGenerator.Persons.PersonDetail.Index(PersonId);

    public void OnGet()
    {
        DeactivateReason = JourneyInstance!.State.DeactivateReason;
        DeactivateReasonDetail = JourneyInstance!.State.DeactivateReasonDetail;
        ReactivateReason = JourneyInstance!.State.ReactivateReason;
        ReactivateReasonDetail = JourneyInstance!.State.ReactivateReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
        ProvideMoreInformation = JourneyInstance!.State.ProvideMoreInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await EvidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);
        await _validator.ValidateAndThrowAsync(this);

        // This is required because EvidenceFile validation (e.g. validation extensions) needs be part
        // of determining if this is valid - which the above does not do.
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ProvideMoreInformation = ProvideMoreInformation;
            state.DeactivateReason = DeactivateReason;
            state.DeactivateReasonDetail = ProvideMoreInformation is ProvideMoreInformationOption.Yes ? DeactivateReasonDetail : null;
            state.ReactivateReason = ReactivateReason;
            state.ReactivateReasonDetail = ProvideMoreInformation is ProvideMoreInformationOption.Yes ? ReactivateReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(LinkGenerator.Persons.PersonDetail.SetStatus.CheckAnswers(PersonId, TargetStatus, JourneyInstance!.InstanceId));
    }
}
