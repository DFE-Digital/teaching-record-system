using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[Journey(JourneyNames.DeleteMq), StartsJourney]
public class IndexModel(
    DeleteMqJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.DeletionReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.DeletionReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.DeletionReason == MqDeletionReasonOption.AnotherReason),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter additional detail")
            .When(m => m.ProvideAdditionalInformation == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    [BindProperty]
    public MqDeletionReasonOption? DeletionReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public string? DeletionReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        Specialism = qualificationInfo.MandatoryQualification.Specialism;

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Qualifications(PersonId);
    }

    public void OnGet()
    {
        DeletionReason = journey.State.DeletionReason;
        DeletionReasonDetail = DeletionReason == MqDeletionReasonOption.AnotherReason ? journey.State.DeletionReasonDetail : null;
        Evidence = journey.State.Evidence;
        AdditionalInformation = ProvideAdditionalInformation == true ? journey.State.AdditionalInformation : null;
        ProvideAdditionalInformation = journey.State.ProvideAdditionalInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        // Upload the evidence file before validating so that it's retained if the form is re-rendered
        // with errors.
        await evidenceUploadManager.UploadAsync(Evidence);

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Mqs.DeleteMq.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.DeletionReason = DeletionReason;
                state.DeletionReasonDetail = DeletionReasonDetail;
                state.Evidence = Evidence;
                state.AdditionalInformation = AdditionalInformation;
                state.ProvideAdditionalInformation = ProvideAdditionalInformation;
            });
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
