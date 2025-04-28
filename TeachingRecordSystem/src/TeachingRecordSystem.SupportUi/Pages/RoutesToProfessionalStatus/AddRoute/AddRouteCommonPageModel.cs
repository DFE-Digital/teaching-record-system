using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class AddRouteCommonPageModel : PageModel
{
    public AddRouteCommonPageModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    {
        _linkGenerator = linkGenerator;
        _referenceDataCache = referenceDataCache;
    }

    protected TrsLinkGenerator _linkGenerator;
    protected ReferenceDataCache _referenceDataCache;

    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public RouteToProfessionalStatus Route { get; set; } = null!;
    public ProfessionalStatusStatus Status { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId));
    }

    public AddRoutePage? NextPage(AddRoutePage currentPage)
    {
        return PageDriver.NextPage(Route, Status, currentPage);
    }

    public AddRoutePage? PreviousPage(AddRoutePage currentPage)
    {
        return PageDriver.PreviousPage(Route, Status, currentPage);
    }

    public bool IsLastPage(AddRoutePage currentPage)
    {
        return PageDriver.IsLastPage(currentPage);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!(JourneyInstance!.State.RouteToProfessionalStatusId.HasValue && JourneyInstance!.State.Status.HasValue))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        Route = await _referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        Status = JourneyInstance!.State.Status!.Value;
        await next();
    }
}
