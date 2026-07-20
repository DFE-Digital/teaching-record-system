using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class ReasonModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.AddReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.AddReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.AddReason == AddMqReasonOption.AnotherReason),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information about why you’re adding this mandatory qualification"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter additional detail")
            .When(m => m.ProvideAdditionalInformation == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public AddMqReasonOption? AddReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? AddReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    public string? AdditionalInformation { get; set; }

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
        await evidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);
        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.AddReason = AddReason;
            state.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.AddReasonDetail = AddReason == AddMqReasonOption.AnotherReason ? AddReasonDetail : null;
            state.Evidence = Evidence;
            state.AdditionalInformation = ProvideAdditionalInformation == true ? AdditionalInformation : null;
        });

        return Redirect(linkGenerator.Mqs.AddMq.CheckAnswers(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.Status is null)
        {
            context.Result = Redirect(linkGenerator.Mqs.AddMq.Status(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
    }
}
