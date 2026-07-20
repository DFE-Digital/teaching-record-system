using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.EditAlertDetails), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
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

    public JourneyInstance<EditAlertDetailsState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

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
        if (JourneyInstance!.State.Details is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.EditAlert.Details.Index(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ProvideAdditionalInformation = JourneyInstance.State.ProvideAdditionalInformation;
        AdditionalInformation = JourneyInstance.State.AdditionalInformation;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await evidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);
        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.ChangeReasonDetail = ChangeReason == AlertChangeDetailsReasonOption.AnotherReason ? ChangeReasonDetail : null;
            state.AdditionalInformation = ProvideAdditionalInformation!.Value ? AdditionalInformation : null;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Alerts.EditAlert.Details.CheckAnswers(AlertId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
