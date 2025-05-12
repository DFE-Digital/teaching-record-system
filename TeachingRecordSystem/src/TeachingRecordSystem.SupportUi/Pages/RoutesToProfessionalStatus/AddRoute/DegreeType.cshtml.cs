using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class DegreeTypeModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.DegreeType) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public DegreeType[] DegreeTypes { get; set; } = [];

    [BindProperty]
    [Display(Name = "Enter the degree type awarded as part of this route")]
    public Guid? DegreeTypeId { get; set; }

    public void OnGet()
    {
        DegreeTypeId = JourneyInstance!.State.DegreeTypeId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (DegreeTypeId is null && Route.DegreeTypeRequired == FieldRequirement.Mandatory)
        {
            ModelState.AddModelError("DegreeTypeId", "Select a degree type");
        }
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.DegreeTypeId = DegreeTypeId);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteAddPage(NextPage(AddRoutePage.DegreeType) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        DegreeTypes = await ReferenceDataCache.GetDegreeTypesAsync();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
