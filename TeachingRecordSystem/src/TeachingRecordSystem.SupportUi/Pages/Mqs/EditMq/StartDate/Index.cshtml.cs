using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

[Journey(JourneyNames.EditMqStartDate), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<EditMqStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Start date")]
    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public void OnGet()
    {
        StartDate = JourneyInstance!.State.StartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (StartDate is null)
        {
            ModelState.AddModelError(nameof(StartDate), "Enter a start date");
        }
        else if (StartDate >= EndDate)
        {
            ModelState.AddModelError(nameof(StartDate), "Start date must be after end date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.StartDate = StartDate);

        return Redirect(linkGenerator.Mqs.EditMq.StartDate.Reason(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        JourneyInstance!.State.EnsureInitialized(qualificationInfo);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        EndDate = qualificationInfo.MandatoryQualification.EndDate;
    }
}
