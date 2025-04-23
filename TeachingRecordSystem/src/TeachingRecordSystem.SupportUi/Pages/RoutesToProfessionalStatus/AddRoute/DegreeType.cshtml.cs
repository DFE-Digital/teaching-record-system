using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class DegreeTypeModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.DegreeType) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public DegreeType[] DegreeTypes { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select a degree type")]
    [Display(Name = "Enter the degree type awarded as part of this route")]
    public Guid? DegreeTypeId { get; set; }

    public void OnGet()
    {
        DegreeTypeId = JourneyInstance!.State.DegreeTypeId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.DegreeTypeId = DegreeTypeId);

        return Redirect(FromCheckAnswers ?
            _linkGenerator.RouteCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.DegreeType) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        DegreeTypes = await _referenceDataCache.GetDegreeTypesAsync();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
