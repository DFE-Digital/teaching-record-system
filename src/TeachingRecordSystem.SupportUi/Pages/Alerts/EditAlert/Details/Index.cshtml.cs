using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[Journey(JourneyNames.EditAlertDetails), StartsJourney]
public class IndexModel(
    EditAlertDetailsJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : PageModel
{
    public const int DetailsMaxLength = 4000;

    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Details)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter details")
            .MaximumLength(DetailsMaxLength).WithMessage("Details must be 4000 characters or less")
            .Must((m, details) => details != m.CurrentDetails).WithMessage("Enter changed details")
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
    public string? Details { get; set; }

    public string? CurrentDetails { get; set; }

    public void OnGet()
    {
        Details = journey.State.Details;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Alerts.EditAlert.Details.Reason(journey.InstanceId),
            state => state.Details = Details);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentDetails = alertInfo.Alert.Details;

        BackLink = journey.GetBackLink() ?? linkGenerator.Alerts.AlertDetail(AlertId);
    }
}
