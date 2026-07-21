using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[Journey(JourneyNames.EditAlertDetails)]
public class ReasonModel(
    EditAlertDetailsJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter details")
            .When(m => m.ProvideAdditionalInformation == true),
        v => v.RuleFor(m => m.Evidence).Evidence(),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.ChangeReason == AlertChangeDetailsReasonOption.AnotherReason),
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public AlertChangeDetailsReasonOption? ChangeReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    [Display(Name = "Add additional detail")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (journey.State.Details is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.EditAlert.Details.Index(journey.InstanceId));
            return;
        }

        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        ChangeReason = journey.State.ChangeReason;
        ProvideAdditionalInformation = journey.State.ProvideAdditionalInformation;
        AdditionalInformation = journey.State.AdditionalInformation;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;
        Evidence = journey.State.Evidence;
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
            linkGenerator.Alerts.EditAlert.Details.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.ChangeReason = ChangeReason;
                state.ProvideAdditionalInformation = ProvideAdditionalInformation;
                state.ChangeReasonDetail = ChangeReason == AlertChangeDetailsReasonOption.AnotherReason ? ChangeReasonDetail : null;
                state.AdditionalInformation = ProvideAdditionalInformation!.Value ? AdditionalInformation : null;
                state.Evidence = Evidence;
            });
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
