using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
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

    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

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
        if (JourneyInstance!.State.StartDate is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.AddAlert.StartDate(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        AddReason = JourneyInstance!.State.AddReason;
        ProvideAdditionalInformation = JourneyInstance.State.ProvideAdditionalInformation;
        AddReasonDetail = JourneyInstance.State.AddReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
        AdditionalInformation = JourneyInstance.State.AdditionalInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        await evidenceUploadManager.UploadAsync(Evidence);

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.AddReason = AddReason;
            state.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.AddReasonDetail = AddReason == AddAlertReasonOption.AnotherReason ? AddReasonDetail : null;
            state.AdditionalInformation = ProvideAdditionalInformation == true ? AdditionalInformation : null;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Alerts.AddAlert.CheckAnswers(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
