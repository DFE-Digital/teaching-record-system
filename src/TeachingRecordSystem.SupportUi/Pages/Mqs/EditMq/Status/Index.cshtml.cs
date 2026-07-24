using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[Journey(JourneyNames.EditMqStatus), StartsJourney]
public class IndexModel(
    EditMqStatusJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.EndDate)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Enter an end date")
            .Must((m, endDate) => !(endDate <= m.StartDate)).WithMessage("End date must be after start date")
            .When(m => m.Status == MandatoryQualificationStatus.Passed),
        v => v.RuleFor(m => m.Status)
            .NotNull().WithMessage("Select a status")
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public MandatoryQualificationStatus? Status { get; set; }

    [BindProperty]
    public DateOnly? EndDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public void OnGet()
    {
        Status = journey.State.Status;
        EndDate = journey.State.EndDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Mqs.EditMq.Status.Reason(journey.InstanceId),
            state =>
            {
                state.Status = Status;
                state.EndDate = Status == MandatoryQualificationStatus.Passed ? EndDate : null;
            });
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        StartDate = qualificationInfo.MandatoryQualification.StartDate;

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Qualifications(PersonId);
    }
}
