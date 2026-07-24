using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq)]
public class StartDateModel(
    AddMqJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<StartDateModel> _validator = new()
    {
        v => v.RuleFor(m => m.StartDate)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Enter a start date")
            .Must((m, startDate) => !(m.EndDate is DateOnly endDate && startDate >= endDate))
                .WithMessage("Start date must be after end date")
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

    public DateOnly? EndDate { get; set; }

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
            linkGenerator.Mqs.AddMq.Status(journey.InstanceId),
            state => state.StartDate = StartDate);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        EndDate = journey.State.EndDate;
    }
}
