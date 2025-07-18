using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class DegreeTypeModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public DegreeType[] DegreeTypes { get; set; } = [];

    [BindProperty]
    public Guid? DegreeTypeId { get; set; }

    public bool DegreeTypeRequired => QuestionDriverHelper.FieldRequired(RouteType!.DegreeTypeRequired, Status.GetDegreeTypeRequirement())
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
            ModelState.AddModelError(nameof(DegreeTypeId), "Select a degree type");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.DegreeTypeId = DegreeTypeId);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        DegreeTypes = await ReferenceDataCache.GetDegreeTypesAsync();
    }
}
