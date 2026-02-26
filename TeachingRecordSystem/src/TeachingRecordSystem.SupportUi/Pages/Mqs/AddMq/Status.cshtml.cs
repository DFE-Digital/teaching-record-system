using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class StatusModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<StatusModel> _validator = new()
    {
        v => v.RuleFor(m => m.Status)
            .NotNull().WithMessage("Select a status")
    };

    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public MandatoryQualificationStatus? Status { get; set; }

    [BindProperty]
    [Display(Name = "End date")]
    public DateOnly? EndDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public void OnGet()
    {
        Status = JourneyInstance!.State.Status;
        EndDate = JourneyInstance!.State.EndDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Status == MandatoryQualificationStatus.Passed)
        {
            if (EndDate is null)
            {
                ModelState.AddModelError(nameof(EndDate), "Enter an end date");
            }
            else if (EndDate <= StartDate)
            {
                ModelState.AddModelError(nameof(EndDate), "End date must be after start date");
            }
        }

        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(
            state =>
            {
                state.Status = Status;
                state.EndDate = Status == MandatoryQualificationStatus.Passed ? EndDate : null;
            });

        return Redirect(linkGenerator.Mqs.AddMq.Reason(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.StartDate is null)
        {
            context.Result = Redirect(linkGenerator.Mqs.AddMq.StartDate(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        StartDate = JourneyInstance!.State.StartDate;
    }
}
