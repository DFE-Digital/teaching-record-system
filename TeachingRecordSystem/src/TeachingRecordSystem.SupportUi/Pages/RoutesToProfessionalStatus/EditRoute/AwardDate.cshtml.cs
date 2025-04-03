using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class AwardDateModel(
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

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Award date")]
    [Required(ErrorMessage = "Enter an award date")]
    [Display(Name = "Enter the professional status award date")]
    public DateOnly? AwardedDate { get; set; }

    public void OnGet()
    {
        AwardedDate = JourneyInstance!.State.AwardedDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.AwardedDate = AwardedDate);

        var route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);

        var hasImplicitExemption = route.InductionExemptionReasonId.HasValue &&
            (await referenceDataCache.GetInductionExemptionReasonByIdAsync(route.InductionExemptionReasonId!.Value)).RouteImplicitExemption;

        if (JourneyInstance!.State.StatusAwardedOrApprovedJourney &&
            route.InductionExemptionRequired == FieldRequirement.Mandatory &&
            !hasImplicitExemption)
        {
            return Redirect(linkGenerator.RouteEditInductionExemption(QualificationId, JourneyInstance.InstanceId));
        }
        else
        {
            return Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
        }
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        base.OnPageHandlerExecuting(context);
    }
}
