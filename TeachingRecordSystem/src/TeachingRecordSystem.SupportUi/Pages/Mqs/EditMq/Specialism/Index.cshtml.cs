using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

[Journey(JourneyNames.EditMqSpecialism), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(ReferenceDataCache referenceDataCache, TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditMqSpecialismState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Specialism")]
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

        return Redirect(linkGenerator.MqEditSpecialismReason(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        JourneyInstance!.State.EnsureInitialized(qualificationInfo);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        var migratedFromDqtWithLegacySpecialism = qualificationInfo.MandatoryQualification.DqtSpecialismId is Guid dqtSpecialismId &&
            MandatoryQualificationSpecialismRegistry.GetByDqtValue((await referenceDataCache.GetMqSpecialismByIdAsync(dqtSpecialismId)).dfeta_Value).IsLegacy();

        Specialisms = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: migratedFromDqtWithLegacySpecialism);

        await next();
    }
}
