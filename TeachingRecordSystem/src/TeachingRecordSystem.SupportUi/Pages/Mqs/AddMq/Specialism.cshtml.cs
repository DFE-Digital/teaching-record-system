using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class SpecialismModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a specialism")]
    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public IReadOnlyCollection<MandatoryQualificationSpecialismInfo>? Specialisms { get; set; }

    public void OnGet()
    {
        Specialism = JourneyInstance!.State.Specialism;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Specialism is MandatoryQualificationSpecialism specialism && !Specialisms!.Any(s => s.Value == specialism))
        {
            ModelState.AddModelError(nameof(Specialism), "Select a valid specialism");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.Specialism = Specialism);

        return Redirect(FromCheckAnswers ?
            linkGenerator.Mqs.AddMq.CheckAnswers(PersonId, JourneyInstance.InstanceId) :
            linkGenerator.Mqs.AddMq.StartDate(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.ProviderId is null)
        {
            context.Result = Redirect(linkGenerator.Mqs.AddMq.Provider(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        Specialisms = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false);

        PersonName = personInfo.Name;
    }
}
