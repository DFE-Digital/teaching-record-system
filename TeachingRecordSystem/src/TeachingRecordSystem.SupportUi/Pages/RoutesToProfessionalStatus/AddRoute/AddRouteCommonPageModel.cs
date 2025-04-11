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

    // currently just uses a knowledge of page order combined with the FieldRequired method
    // page will also need to know whether the route can have an exemption (if status is awarded/approved)
    // and also need hasImplicitexemption - from InductionExemptionReason
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
        var lastPage = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .OrderByDescending(p => p)
            .First();
        return lastPage == currentPage;
    }

    public string PageAddress(AddRoutePage page)
    {
        return page switch
        {
            AddRoutePage.EndDate => _linkGenerator.RouteAddEndDate(PersonId, JourneyInstance!.InstanceId),
            AddRoutePage.StartDate => _linkGenerator.RouteAddStartDate(PersonId, JourneyInstance!.InstanceId),
            AddRoutePage.Status => _linkGenerator.RouteAddStatus(PersonId, JourneyInstance!.InstanceId),
            AddRoutePage.Route => _linkGenerator.RouteAddRoute(PersonId, JourneyInstance!.InstanceId),
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!(JourneyInstance!.State.RouteToProfessionalStatusId.HasValue && JourneyInstance!.State.Status.HasValue))
        {
            context.Result = new BadRequestResult();
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        Route = await _referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId!.Value);
        Status = JourneyInstance!.State.Status!.Value;
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
