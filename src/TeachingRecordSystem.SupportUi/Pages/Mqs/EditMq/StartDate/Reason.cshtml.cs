using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

[Journey(JourneyNames.EditMqStartDate), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.ChangeReason == MqChangeStartDateReasonOption.AnotherReason),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter additional detail")
            .When(m => m.ProvideAdditionalInformation == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public JourneyInstance<EditMqStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public MqChangeStartDateReasonOption? ChangeReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.StartDate is null)
        {
            context.Result = Redirect(linkGenerator.Mqs.EditMq.StartDate.Index(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = ChangeReason == MqChangeStartDateReasonOption.AnotherReason ? JourneyInstance.State.ChangeReasonDetail : null;
        Evidence = JourneyInstance.State.Evidence;
        AdditionalInformation = JourneyInstance!.State.ProvideAdditionalInformation == true ? JourneyInstance!.State.AdditionalInformation : null;
        ProvideAdditionalInformation = JourneyInstance!.State.ProvideAdditionalInformation;
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
            state.ChangeReasonDetail = ChangeReason == MqChangeStartDateReasonOption.AnotherReason ? ChangeReasonDetail : null;
            state.Evidence = Evidence;
            state.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.AdditionalInformation = ProvideAdditionalInformation == true ? AdditionalInformation : null;

        });

        return Redirect(linkGenerator.Mqs.EditMq.StartDate.CheckAnswers(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
