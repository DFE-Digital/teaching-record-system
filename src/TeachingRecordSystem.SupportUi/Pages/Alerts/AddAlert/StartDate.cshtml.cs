using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert)]
public class StartDateModel(
    AddAlertJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<StartDateModel> _validator = new()
    {
        v => v.RuleFor(m => m.StartDate).AlertStartDate(
            timeProvider.Today,
            requiredMessage: "Enter a start date",
            dateInFutureMessage: "Start date cannot be in the future")
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public DateOnly? StartDate { get; set; }

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
            linkGenerator.Alerts.AddAlert.Reason(journey.InstanceId),
            state => state.StartDate = StartDate!.Value);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
    }
}
