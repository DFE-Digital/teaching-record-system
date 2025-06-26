using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public abstract class AddRoutePostStatusPageModel(AddRoutePage currentPage, TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(currentPage, linkGenerator, referenceDataCache)
{
    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public RouteToProfessionalStatusStatus Status { get; set; }

    public override AddRoutePage? NextPage => PageDriver.NextPage(Route, Status, CurrentPage);

    public override AddRoutePage? PreviousPage => PageDriver.PreviousPage(Route, Status, CurrentPage);

    public bool IsLastPage => PageDriver.IsLastPage(CurrentPage);

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        if (!(JourneyInstance!.State.RouteToProfessionalStatusId.HasValue && JourneyInstance!.State.Status.HasValue))
        {
            context.Result = BadRequest();
            return;
        }

        Route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        Status = JourneyInstance!.State.Status!.Value;

        await base.OnPageHandlerExecutingAsync(context);
    }
}
