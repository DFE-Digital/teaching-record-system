using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

[Journey(JourneyNames.EditMqSpecialism), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditMqSpecialismState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Specialism")]
    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationSpecialismInfo[]? Specialisms { get; set; }

    public void OnGet()
    {
        Specialism ??= JourneyInstance!.State.Specialism;
    }

    public async Task<IActionResult> OnPost()
    {
        if (Specialism is null)
        {
            ModelState.AddModelError(nameof(Specialism), "Select a specialism");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.Specialism = Specialism);

        return Redirect(linkGenerator.MqEditSpecialismConfirm(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var qualification = await crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId));
        if (qualification is null || qualification.dfeta_Type != dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            context.Result = NotFound();
            return;
        }

        Specialisms = MandatoryQualificationSpecialismRegistry.All
            .OrderBy(t => t.Title)
            .ToArray();

        await JourneyInstance!.State.EnsureInitialized(crmQueryDispatcher, referenceDataCache, qualification);

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;

        await next();
    }
}
