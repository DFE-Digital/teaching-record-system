using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class DegreeTypeModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(RoutePage.DegreeType) ?? RoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public DegreeType[] DegreeTypes { get; set; } = [];

    public string PageHeading => "Enter the degree type awarded as part of this route" + (!DegreeTypeRequired ? " (optional)" : "");
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
        if (DegreeTypeId is null && DegreeTypeRequired)
        {
            ModelState.AddModelError("DegreeTypeId", "Select a degree type");
        }
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.DegreeTypeId = DegreeTypeId);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteAddPage(NextPage(RoutePage.DegreeType) ?? RoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        DegreeTypes = await ReferenceDataCache.GetDegreeTypesAsync();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
