using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[Journey(JourneyNames.EditMqStatus)]
public class ReasonModel(
    EditMqStatusJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.EndDateChangeReason)
            .NotNull().WithMessage("Select a reason")
            .When(m => m.IsEndDateChange == true && m.IsStatusChange == false),
        v => v.RuleFor(m => m.StatusChangeReason)
            .NotNull().WithMessage("Select a reason")
            .When(m => m.IsStatusChange == true),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information"),
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

    [BindProperty]
    public MqChangeStatusReasonOption? StatusChangeReason { get; set; }

    [BindProperty]
    public MqChangeEndDateReasonOption? EndDateChangeReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool? IsEndDateChange { get; set; }

    public bool? IsStatusChange { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        IsEndDateChange = journey.State.IsEndDateChange;
        IsStatusChange = journey.State.IsStatusChange;
    }

    public void OnGet()
    {
        StatusChangeReason = journey.State.StatusChangeReason;
        EndDateChangeReason = journey.State.EndDateChangeReason;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;
        Evidence = journey.State.Evidence;
        ProvideAdditionalInformation = journey.State.ProvideAdditionalInformation;
        AdditionalInformation = ProvideAdditionalInformation == true ? journey.State.AdditionalInformation : null;
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
            linkGenerator.Mqs.EditMq.Status.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.StatusChangeReason = StatusChangeReason;
                state.EndDateChangeReason = EndDateChangeReason;
                state.ChangeReasonDetail = ChangeReasonDetail;
                state.Evidence = Evidence;
                state.ProvideAdditionalInformation = ProvideAdditionalInformation;
                state.AdditionalInformation = ProvideAdditionalInformation == true ? AdditionalInformation : null;
            });
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
