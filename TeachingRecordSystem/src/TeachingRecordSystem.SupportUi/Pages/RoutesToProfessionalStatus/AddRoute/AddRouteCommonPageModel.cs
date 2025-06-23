using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public abstract class AddRouteCommonPageModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : PageModel
{
    protected TrsLinkGenerator LinkGenerator => linkGenerator;

    protected ReferenceDataCache ReferenceDataCache => referenceDataCache;

    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public RouteToProfessionalStatusType Route { get; set; } = null!;
    public RouteToProfessionalStatusStatus Status { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonQualifications(PersonId));
    }

    protected abstract RoutePage CurrentPage { get; }

    protected string NextPage =>
        PageDriver.NextPage(Route, Status, CurrentPage, FromCheckAnswers) is RoutePage nextPage && nextPage == CurrentPage
            ? LinkGenerator.PersonQualifications(PersonId)
            : LinkGenerator.RouteAddPage(nextPage, PersonId, JourneyInstance!.InstanceId);

    protected string PreviousPage =>
        PageDriver.PreviousPage(Route, Status, CurrentPage, FromCheckAnswers) is RoutePage previousPage && previousPage == CurrentPage
            ? LinkGenerator.PersonQualifications(PersonId)
            : LinkGenerator.RouteAddPage(previousPage, PersonId, JourneyInstance!.InstanceId);

    //public bool IsLastPage(AddRoutePage currentPage)
    //{
    //    return PageDriver.IsLastPage(currentPage);
    //}

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (CurrentPage > RoutePage.Route && !JourneyInstance!.State.RouteToProfessionalStatusId.HasValue ||
            CurrentPage > RoutePage.Status && !JourneyInstance!.State.Status.HasValue)
        {
            context.Result = new BadRequestResult();
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        if (JourneyInstance!.State.RouteToProfessionalStatusId.HasValue)
        {
            Route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        }

        if (JourneyInstance!.State.Status.HasValue)
        {
            Status = JourneyInstance!.State.Status.Value;
        }

        await next();
    }
}
