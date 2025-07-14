using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class DegreeTypeModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.DegreeType, linkGenerator, referenceDataCache)
{
    public string PageTitle = "Add degree type";
    public string PageHeading = "Enter the degree type awarded as part of this route";

    public DegreeType[] DegreeTypes { get; set; } = [];

    public bool DegreeTypeRequired => QuestionDriverHelper.FieldRequired(Route.DegreeTypeRequired, Status.GetDegreeTypeRequirement())
        == FieldRequirement.Mandatory;

    [BindProperty]
    public Guid? DegreeTypeId { get; set; }

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
