using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[Journey(JourneyNames.EditAlertDetails), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceController) : PageModel
{
    public const int DetailsMaxLength = 4000;

    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Details)
            .NotEmpty().WithMessage("Enter details")
            .MaximumLength(DetailsMaxLength).WithMessage("Details must be 4000 characters or less")
    };

    public JourneyInstance<EditAlertDetailsState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public string? Details { get; set; }

    public string? CurrentDetails { get; set; }

    public void OnGet()
    {
        Details = JourneyInstance!.State.Details;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Details == CurrentDetails)
        {
            ModelState.AddModelError(nameof(Details), "Enter changed details");
        }

        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Details = Details;
        });

        return Redirect(FromCheckAnswers
            ? linkGenerator.Alerts.EditAlert.Details.CheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.Alerts.EditAlert.Details.Reason(AlertId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        JourneyInstance!.State.EnsureInitialized(alertInfo);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentDetails = alertInfo.Alert.Details;
    }
}
