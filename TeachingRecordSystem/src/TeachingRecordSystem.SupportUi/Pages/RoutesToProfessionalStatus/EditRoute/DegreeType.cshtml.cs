using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class DegreeTypeModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswer { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    public DegreeType[]? DegreeTypes { get; set; }

    [Required]
    [BindProperty]
    public Guid? DegreeTypeId { get; init; }

    public async Task<IActionResult> OnGetAsync()
    {
        DegreeTypes = await referenceDataCache.GetDegreeTypesAsync();
        return Page();
    }

    public async Task<IActionResult>OnPostAsync()
    {
        JourneyInstance!.State.DegreeTypeId = DegreeTypeId!.Value;
        await JourneyInstance!.UpdateStateAsync(s => s.DegreeTypeId = DegreeTypeId!.Value);

        return Redirect(FromCheckAnswer ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
    }
}
