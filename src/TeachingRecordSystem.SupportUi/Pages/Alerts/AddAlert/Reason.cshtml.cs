using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert)]
public class ReasonModel(
    AddAlertJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.AddReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotNull().WithMessage("Enter details")
            .MaximumLength(4000).WithMessage("Additional detail must be 4000 characters or less")
            .When(m => m.ProvideAdditionalInformation == true),
        v => v.RuleFor(m => m.Evidence.UploadEvidence)
            .NotNull().WithMessage("Select yes if you want to upload evidence"),
        v => v.RuleFor(m => m.Evidence).Evidence(),
        v => v.RuleFor(m => m.AddReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.AddReason == AddAlertReasonOption.AnotherReason),
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public AddAlertReasonOption? AddReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? AddReasonDetail { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (journey.State.StartDate is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.AddAlert.StartDate(journey.InstanceId));
            return;
        }

        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        AddReason = journey.State.AddReason;
        ProvideAdditionalInformation = journey.State.ProvideAdditionalInformation;
        AddReasonDetail = journey.State.AddReasonDetail;
        Evidence = journey.State.Evidence;
        AdditionalInformation = journey.State.AdditionalInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        await evidenceUploadManager.UploadAsync(Evidence);

        return journey.AdvanceTo(
            linkGenerator.Alerts.AddAlert.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.AddReason = AddReason;
                state.ProvideAdditionalInformation = ProvideAdditionalInformation;
                state.AddReasonDetail = AddReason == AddAlertReasonOption.AnotherReason ? AddReasonDetail : null;
                state.AdditionalInformation = ProvideAdditionalInformation == true ? AdditionalInformation : null;
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
