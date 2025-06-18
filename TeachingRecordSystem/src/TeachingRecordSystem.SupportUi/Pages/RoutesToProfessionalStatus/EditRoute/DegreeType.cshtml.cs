using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class DegreeTypeModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    public DegreeType[] DegreeTypes { get; set; } = [];

    public RouteToProfessionalStatusType? RouteToProfessionalStatusType { get; set; }

    [BindProperty]
    [Display(Name = "Enter the degree type awarded as part of this route")]
    public Guid? DegreeTypeId { get; set; }

    public void OnGet()
    {
        DegreeTypeId = JourneyInstance!.State.DegreeTypeId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var fieldRequirement = QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType!.DegreeTypeRequired, JourneyInstance!.State.Status.GetDegreeTypeRequirement());
        if (fieldRequirement == FieldRequirement.Mandatory && DegreeTypeId is null)
        {
            ModelState.AddModelError(nameof(DegreeTypeId), "Select a degree type");
        }
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.DegreeTypeId = DegreeTypeId);

        return Redirect(FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
        var routeInfo = context.HttpContext.GetCurrentProfessionalStatusFeature();
        RouteToProfessionalStatusType = routeInfo.RouteToProfessionalStatus.RouteToProfessionalStatusType;
        DegreeTypes = await referenceDataCache.GetDegreeTypesAsync();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
