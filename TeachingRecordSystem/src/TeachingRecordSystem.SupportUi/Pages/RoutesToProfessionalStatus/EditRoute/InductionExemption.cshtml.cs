using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class InductionExemptionModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }
    public RouteToProfessionalStatus Route { get; set; } = null!;
    public InductionExemptionReason? ExemptionReason { get; set; }
    public string BackLink => linkGenerator.PersonQualifications(PersonId);

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    [Display(Name = "Does this route provide them with an induction exemption?")]
    [Required(ErrorMessage = "Select yes if this route provides an induction exemption")]
    public bool IsExemptFromInduction { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        //Guid? exemptionReasonId = IsExemptFromInduction ?
        //    Route.InductionExemptionReasonId
        //    : null;
        //await JourneyInstance!.UpdateStateAsync(s => s.InductionExemptionReasonId = exemptionReasonId);

        await JourneyInstance!.UpdateStateAsync(s => s.IsExemptFromInduction = IsExemptFromInduction);
        return Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId));
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId));
    }

    public async override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        if (Route.InductionExemptionReasonId is not null)
        {
            ExemptionReason = await referenceDataCache.GetInductionExemptionReasonByIdAsync(Route.InductionExemptionReasonId!.Value);
            if (ExemptionReason.RouteImplicitExemption)
            {
                throw new InvalidOperationException("This route provides an implict induction exemption");
            }
        }
        else
        {
            throw new InvalidOperationException("This route does not provide an induction exemption");
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
