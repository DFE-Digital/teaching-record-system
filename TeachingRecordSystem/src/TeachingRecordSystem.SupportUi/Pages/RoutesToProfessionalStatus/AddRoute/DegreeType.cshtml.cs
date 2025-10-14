using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class DegreeTypeModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : AddRoutePostStatusPageModel(AddRoutePage.DegreeType, linkGenerator, referenceDataCache, evidenceController)
{
    public DegreeType[] DegreeTypes { get; set; } = [];

    [BindProperty]
    public Guid? DegreeTypeId { get; set; }

    public bool DegreeTypeRequired => QuestionDriverHelper.FieldRequired(RouteType.DegreeTypeRequired, Status.GetDegreeTypeRequirement())
        == FieldRequirement.Mandatory;

    public string PageHeading => "Enter the degree type awarded as part of this route"
       + (DegreeTypeRequired ? "" : " (optional)");

    public void OnGet()
    {
        DegreeTypeId = JourneyInstance!.State.DegreeTypeId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (DegreeTypeRequired && DegreeTypeId is null)
        {
            ModelState.AddModelError("DegreeTypeId", "Select a degree type");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.DegreeTypeId = DegreeTypeId;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        DegreeTypes = await ReferenceDataCache.GetDegreeTypesAsync();
    }
}
