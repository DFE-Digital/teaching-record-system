using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus)]
[AllowDeactivatedPerson]
public class ReasonModel(
    SetStatusJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
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
            .When(x => x.DeactivateReason ==  PersonDeactivateReason.AnotherReason && x.TargetStatus == PersonStatus.Deactivated),

        v => v.RuleFor(m => m.DeactivateAdditionalInformation)
            .NotEmpty()
            .WithMessage("Enter details")
            .When(x => x.ProvideMoreInformation == ProvideMoreInformationOption.Yes && x.TargetStatus == PersonStatus.Deactivated),

        v => v.RuleFor(m => m.DeactivateAdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
            .WithMessage($"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}")
            .When(m => m.TargetStatus == PersonStatus.Deactivated),
        
        //re-activate rules
        v => v.RuleFor(m => m.ReactivateReason)
            .NotEmpty()
            .WithMessage("Select a reason")
            .When(x=>x.TargetStatus == PersonStatus.Active),

        v => v.RuleFor(m => m.ProvideMoreInformation)
            .NotNull()
            .WithMessage("Select yes if you want to provide more information")
            .When(x=>x.TargetStatus == PersonStatus.Active),

        v => v.RuleFor(m => m.ReactivateAdditionalInformation)
            .NotEmpty()
            .WithMessage("Enter details")
            .When(x => x.ProvideMoreInformation == ProvideMoreInformationOption.Yes && x.TargetStatus == PersonStatus.Active),

        v => v.RuleFor(m => m.ReactivateAdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}")
            .When(m => m.TargetStatus == PersonStatus.Active && m.ProvideMoreInformation == ProvideMoreInformationOption.Yes),

        v => v.RuleFor(m => m.ReactivateReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.ReactivateReason == PersonReactivateReason.AnotherReason),
        
        // Make sure to take into account evidence models validation rules.
        v => v.RuleFor(x => x.Evidence).Evidence(),

        v => v.RuleFor(m => m.ReactivateReasonDetail)
            .NotEmpty()
            .WithMessage("Enter a reason")
            .When(x => x.DeactivateReason ==  PersonDeactivateReason.AnotherReason && x.TargetStatus == PersonStatus.Active),
    };

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromRoute]
    public PersonStatus TargetStatus { get; set; }

    public string? PersonName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Trn { get; set; }

    public Gender? Gender { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public string? EmailAddress { get; set; }

    public PersonStatus? Status { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public PersonDeactivateReason? DeactivateReason { get; set; }

    [BindProperty]
    public ProvideMoreInformationOption? ProvideMoreInformation { get; set; }

    [BindProperty]
    public string? DeactivateReasonDetail { get; set; }

    [BindProperty]
    public PersonReactivateReason? ReactivateReason { get; set; }

    [BindProperty]
    public string? ReactivateReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    public string? ReactivateAdditionalInformation { get; set; }

    [BindProperty]
    public string? DeactivateAdditionalInformation { get; set; }

    public void OnGet()
    {
        DeactivateReason = journey.State.DeactivateReason;
        DeactivateReasonDetail = journey.State.DeactivateReasonDetail;
        DeactivateAdditionalInformation = journey.State.DeactivateAdditionalInformation;
        ReactivateReason = journey.State.ReactivateReason;
        ReactivateReasonDetail = journey.State.ReactivateReasonDetail;
        ReactivateAdditionalInformation = journey.State.ReactivateAdditionalInformation;
        Evidence = journey.State.Evidence;
        ProvideMoreInformation = journey.State.ProvideMoreInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        // Upload the evidence file before validating so that it's retained if the form is re-rendered
        // with errors.
        await evidenceUploadManager.UploadAsync(Evidence);

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Persons.PersonDetail.SetStatus.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.ProvideMoreInformation = ProvideMoreInformation;
                state.DeactivateReason = DeactivateReason;
                state.DeactivateReasonDetail = DeactivateReason is PersonDeactivateReason.AnotherReason ? DeactivateReasonDetail : null;
                state.DeactivateAdditionalInformation = ProvideMoreInformation is ProvideMoreInformationOption.Yes ? DeactivateAdditionalInformation : null;
                state.ReactivateReason = ReactivateReason;
                state.ReactivateReasonDetail = ReactivateReason is PersonReactivateReason.AnotherReason ? ReactivateReasonDetail : null;
                state.ReactivateAdditionalInformation = ProvideMoreInformation is ProvideMoreInformationOption.Yes ? ReactivateAdditionalInformation : null;
                state.Evidence = Evidence;
            });
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        Status = personInfo.Status;

        var person = await journey.GetPersonAsync();

        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        if (!journey.StatusChangeIsApplicable(person))
        {
            context.Result = BadRequest();
            return;
        }

        NationalInsuranceNumber = person.NationalInsuranceNumber;
        Trn = person.Trn;
        Gender = person.Gender;
        DateOfBirth = person.DateOfBirth;
        EmailAddress = person.EmailAddress;

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Index(PersonId);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
