using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

[Journey(JourneyNames.EditAlertStartDate), StartsJourney]
public class IndexModel(
    EditAlertStartDateJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.StartDate)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Enter a start date")
            .LessThanOrEqualTo(timeProvider.Today).WithMessage("Start date cannot be in the future")
            .Must((m, startDate) => startDate != m.PreviousStartDate).WithMessage("Enter a different start date")
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
    public DateOnly? StartDate { get; set; }

    public DateOnly? PreviousStartDate { get; set; }

    public void OnGet()
    {
        StartDate = journey.State.StartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Alerts.EditAlert.StartDate.Reason(journey.InstanceId),
            state => state.StartDate = StartDate);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        PreviousStartDate = alertInfo.Alert.StartDate;

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Alerts(PersonId);
    }
}
